using System.Runtime.CompilerServices;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Logging;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Relics;
using RevertDiamondDiadem.Models;
using STS2RitsuLib.Patching.Models;

namespace RevertDiamondDiadem.Patches;

/// <summary>
/// 本回合出牌计数的存储与遗物状态刷新。
/// 原私有字段 _cardsPlayedThisTurn 无法补回也不宜触碰存档序列化，
/// 故用 ConditionalWeakTable 以遗物实例为键持有计数（战斗实例生命周期内有效）；
/// 阈值写死为 107 官方值（系列“彻底写死”惯例）。
/// </summary>
internal static class DiademState
{
    internal const decimal Threshold = 2m;

    private static readonly ConditionalWeakTable<DiamondDiadem, StrongBox<int>> Counts = new();

    internal static int Get(DiamondDiadem relic) => Counts.TryGetValue(relic, out var box) ? box.Value : 0;

    internal static void Increment(DiamondDiadem relic)
    {
        var box = Counts.GetOrCreateValue(relic);
        box.Value++;
    }

    internal static void Reset(DiamondDiadem relic)
    {
        if (Counts.TryGetValue(relic, out var box))
        {
            box.Value = 0;
        }
    }

    /// <summary>复刻 107 RefreshCounter：≤阈值高亮 Active，并刷新遗物旁的数字。</summary>
    internal static void RefreshStatus(DiamondDiadem relic)
    {
        relic.Status = Get(relic) <= Threshold ? RelicStatus.Active : RelicStatus.Normal;
        relic.InvokeDisplayAmountChanged();
    }
}

/// <summary>
/// 出牌监听：复刻 107 AfterCardPlayed——只统计本人出的牌且要求战斗进行中。
/// 该钩子挂 RelicModel 虚声明处，postfix 内按实例类型过滤。
/// </summary>
public sealed class CardCounterPatch : IPatchMethod
{
    public static string PatchId => "reddia_card_counter";
    public static string Description => "Count the owner's cards played this turn for diamond diadem";
    public static bool IsCritical => true;

    public static ModPatchTarget[] GetTargets() =>
    [
        PatchTarget.OptionalMethod(typeof(RelicModel), "AfterCardPlayed",
            typeof(PlayerChoiceContext), typeof(CardPlay)),
    ];

    public static void Postfix(RelicModel __instance, CardPlay cardPlay)
    {
        if (!HostShapeGate.Active || __instance is not DiamondDiadem relic)
        {
            return;
        }

        if (cardPlay.Card.Owner != relic.Owner)
        {
            return;
        }

        if (!CombatManager.Instance.IsInProgress)
        {
            return;
        }

        DiademState.Increment(relic);
        DiademState.RefreshStatus(relic);
    }
}

/// <summary>
/// 回合结束结算：复刻 107 判定——本回合打出 ≤2 张牌时施加专属减伤 buff；无论是否触发都清零计数。
/// Prefix 全替换原 Task 返回值（异步方法级组合，与回调系 SynchronizeOnPlay 同形态）。
/// </summary>
public sealed class TurnEndTriggerPatch : IPatchMethod
{
    public static string PatchId => "reddia_turn_end_trigger";
    public static string Description => "Apply legacy buff at turn end when <=2 cards were played this turn";
    public static bool IsCritical => true;

    public static ModPatchTarget[] GetTargets() =>
    [
        PatchTarget.OptionalMethod(typeof(RelicModel), "BeforeSideTurnEnd",
            typeof(PlayerChoiceContext), typeof(CombatSide), typeof(IEnumerable<Creature>)),
    ];

    public static bool Prefix(
        RelicModel __instance,
        PlayerChoiceContext choiceContext,
        IEnumerable<Creature> participants,
        ref Task __result)
    {
        if (!HostShapeGate.Active || __instance is not DiamondDiadem relic)
        {
            return true;
        }

        __result = RunAsync(relic, choiceContext, participants);
        return false;
    }

    private static bool _loggedFirstTrigger;

    private static async Task RunAsync(
        DiamondDiadem relic,
        PlayerChoiceContext choiceContext,
        IEnumerable<Creature> participants)
    {
        if (participants.Contains(relic.Owner.Creature))
        {
            var played = (decimal)DiademState.Get(relic);
            if (!_loggedFirstTrigger)
            {
                _loggedFirstTrigger = true;
                Log.Info($"{ModEntry.ModId}: turn-end trigger live, cards played this turn = {played}");
            }

            if (played <= DiademState.Threshold)
            {
                relic.Flash();
                try
                {
                    await PowerCmd.Apply<RevertDiamondDiademPower>(
                        choiceContext, relic.Owner.Creature, 1m, relic.Owner.Creature, null);
                }
                catch (Exception e)
                {
                    Log.Error($"{ModEntry.ModId}: failed to apply legacy buff: {e}");
                }
            }

            // 复刻 107 语义：无论是否触发都清零，且必须刷新计数显示与 Active 高亮
            // （107 的 RefreshCounter 在清零后调用 InvokeDisplayAmountChanged；漏掉会导致 UI 停留在旧值）
            DiademState.Reset(relic);
            DiademState.RefreshStatus(relic);
        }
    }
}

/// <summary>遗物图标旁显示“本回合已出牌数”，战斗外隐藏（107 版 ShowCounter 语义）。</summary>
public sealed class ShowCounterUiPatch : IPatchMethod
{
    public static string PatchId => "reddia_show_counter_ui";
    public static string Description => "Restore ShowCounter of v0.107 for diamond diadem";
    public static bool IsCritical => false;

    public static ModPatchTarget[] GetTargets() =>
    [
        PatchTarget.Getter(typeof(RelicModel), "ShowCounter"),
    ];

    public static void Postfix(RelicModel __instance, ref bool __result)
    {
        if (!HostShapeGate.Active || __instance is not DiamondDiadem)
        {
            return;
        }

        __result = CombatManager.Instance.IsInProgress;
    }
}

public sealed class DisplayAmountUiPatch : IPatchMethod
{
    public static string PatchId => "reddia_display_amount_ui";
    public static string Description => "Restore DisplayAmount of v0.107 for diamond diadem";
    public static bool IsCritical => false;

    public static ModPatchTarget[] GetTargets() =>
    [
        PatchTarget.Getter(typeof(RelicModel), "DisplayAmount"),
    ];

    public static void Postfix(RelicModel __instance, ref int __result)
    {
        if (!HostShapeGate.Active || __instance is not DiamondDiadem relic)
        {
            return;
        }

        __result = DiademState.Get(relic);
    }
}
