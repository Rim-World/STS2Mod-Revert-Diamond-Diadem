using System.Reflection;
using MegaCrit.Sts2.Core.Logging;
using MegaCrit.Sts2.Core.Modding;
using RevertDiamondDiadem.Patches;
using STS2RitsuLib;
using STS2RitsuLib.Interop;
using STS2RitsuLib.Patching.Core;

namespace RevertDiamondDiadem;

/// <summary>
/// 入口：先做宿主形态判定（行为级判据，不用版本号），仅当宿主的钻石头冠是新效果（beta 111 式）时
/// 才创建 patcher 注册恢复组补丁；default 107 等旧效果宿主上整个 mod 为零补丁 no-op。
/// </summary>
[ModInitializer(nameof(Initialize))]
public static class ModEntry
{
    public const string ModId = "RevertDiamondDiadem";

    public static void Initialize()
    {
        try
        {
            var assembly = Assembly.GetExecutingAssembly();
            ModTypeDiscoveryHub.RegisterModAssembly(ModId, assembly);
            var log = RitsuLibFramework.CreateLogger(ModId);

            if (!HostShapeGate.Evaluate(log))
            {
                return;
            }

            var patcher = RitsuLibFramework.CreatePatcher(ModId, "revert_main");
            patcher.RegisterPatch<SideTurnStartBypassPatch>();
            patcher.RegisterPatch<BlockHoverTipRemovalPatch>();
            patcher.RegisterPatch<CardCounterPatch>();
            patcher.RegisterPatch<TurnEndTriggerPatch>();
            patcher.RegisterPatch<ShowCounterUiPatch>();
            patcher.RegisterPatch<DisplayAmountUiPatch>();
            patcher.RegisterPatch<HalfDamagePatch>();
            patcher.RegisterPatch<LegacyRelicDescriptionPatch>();
            patcher.RegisterPatch<PowerSmallIconPatch>();
            patcher.RegisterPatch<PowerBigIconPatch>();
            RitsuLibFramework.ApplyRequiredPatcher(patcher, () =>
            {
                Log.Error($"{ModId}: required revert patches failed to apply; disabling mod behavior.");
                HostShapeGate.Disable();
            });

            log.Info("initialized; diamond diadem reverted to legacy (v0.107) behavior.");
        }
        catch (Exception e)
        {
            Log.Error($"{ModId}: failed to initialize: {e}");
        }
    }
}
