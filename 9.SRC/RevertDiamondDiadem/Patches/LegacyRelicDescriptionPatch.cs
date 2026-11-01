using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Relics;
using STS2RitsuLib.Patching.Models;

namespace RevertDiamondDiadem.Patches;

/// <summary>
/// 遗物描述改读本 mod 独立键 RE_DIAMOND_DIADEM.description（内容为 v0.107 官方文案，占位符已按惯例硬编码为 2），
/// 不覆写官方同键，避免与其他覆写同一官方键的 mod 发生合并竞争（系列先例：独立 RE_ 键）。
/// title/flavor 两版本一致，无需处理。
/// </summary>
public sealed class LegacyRelicDescriptionPatch : IPatchMethod
{
    public const string Key = "RE_DIAMOND_DIADEM.description";

    public static string PatchId => "reddia_legacy_description";
    public static string Description => "Point diamond diadem description at the legacy text key";
    public static bool IsCritical => false;

    public static ModPatchTarget[] GetTargets() =>
    [
        PatchTarget.Getter(typeof(RelicModel), "Description"),
    ];

    public static void Postfix(RelicModel __instance, ref LocString __result)
    {
        if (!HostShapeGate.Active || __instance is not DiamondDiadem)
        {
            return;
        }

        __result = new LocString("relics", Key);
    }
}
