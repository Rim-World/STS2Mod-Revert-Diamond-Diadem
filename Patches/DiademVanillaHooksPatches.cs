using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Models.Relics;
using STS2RitsuLib.Patching.Models;

namespace RevertDiamondDiadem.Patches;

/// <summary>
/// 摘除 beta 111 版钻石头冠的新触发钩子本体（20 点格挡 + BlurPower）：
/// 仅在战斗第 1 个玩家回合开始时生效一次。形态门保证本补丁只在 DiamondDiadem 自身声明了该方法的宿主上注册。
/// </summary>
public sealed class SideTurnStartBypassPatch : IPatchMethod
{
    public static string PatchId => "reddia_side_turn_start_bypass";
    public static string Description => "Disable vanilla diamond diadem start-of-combat block + blur";
    public static bool IsCritical => true;

    public static ModPatchTarget[] GetTargets() =>
    [
        PatchTarget.OptionalMethod(typeof(DiamondDiadem), "AfterSideTurnStart",
            typeof(CombatSide), typeof(IReadOnlyList<Creature>), typeof(ICombatState)),
    ];

    public static bool Prefix(ref Task __result)
    {
        if (!HostShapeGate.Active)
        {
            return true;
        }

        __result = Task.CompletedTask;
        return false;
    }
}

/// <summary>
/// 摘除 111 版新增的格挡图标悬停提示（ExtraHoverTips），使遗物悬停信息与 107 减伤语义一致。
/// </summary>
public sealed class BlockHoverTipRemovalPatch : IPatchMethod
{
    public static string PatchId => "reddia_block_hover_tip_removal";
    public static string Description => "Remove the block hover tip added in beta for diamond diadem";
    public static bool IsCritical => false;

    public static ModPatchTarget[] GetTargets() =>
    [
        PatchTarget.OptionalGetter(typeof(DiamondDiadem), "ExtraHoverTips"),
    ];

    public static bool Prefix(ref IEnumerable<IHoverTip> __result)
    {
        if (!HostShapeGate.Active)
        {
            return true;
        }

        __result = Enumerable.Empty<IHoverTip>();
        return false;
    }
}
