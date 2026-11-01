using MegaCrit.Sts2.Core.Models;
using RevertDiamondDiadem.Models;
using STS2RitsuLib.Patching.Models;

namespace RevertDiamondDiadem.Patches;

/// <summary>
/// 自定义 buff 的图标指回官方原类资源。游戏按类型名 slug 推导图标路径：
/// 我们的类名带 Revert 前缀（slug=revert_diamond_diadem_power），找不到官方为原类
/// DiamondDiademPower 生成的 diamond_diadem_power 图集条目，于是回落到 missing 占位图。
/// 经 pck-tool 实证，107/111 官方包中原类的图标资源（图集 tres + powers 大图）均未删除，
/// 故把两个图标路径改写为官方原类资源路径即可——走官方 remap 加载，无需自带任何图片。
/// </summary>
public sealed class PowerSmallIconPatch : IPatchMethod
{
    public const string OfficialAtlasTresPath =
        "res://images/atlases/power_atlas.sprites/diamond_diadem_power.tres";

    public static string PatchId => "reddia_power_small_icon";
    public static string Description => "Point the legacy buff's HUD icon at the official atlas entry of the original power";
    public static bool IsCritical => false;

    public static ModPatchTarget[] GetTargets() =>
    [
        PatchTarget.Getter(typeof(PowerModel), "PackedIconPath"),
    ];

    public static void Postfix(PowerModel __instance, ref string __result)
    {
        if (!HostShapeGate.Active || __instance is not RevertDiamondDiademPower)
        {
            return;
        }

        __result = OfficialAtlasTresPath;
    }
}

public sealed class PowerBigIconPatch : IPatchMethod
{
    public const string OfficialBigIconPath = "res://images/powers/diamond_diadem_power.png";

    public static string PatchId => "reddia_power_big_icon";
    public static string Description => "Point the legacy buff's tooltip big icon at the official png of the original power";
    public static bool IsCritical => false;

    public static ModPatchTarget[] GetTargets() =>
    [
        PatchTarget.Getter(typeof(PowerModel), "ResolvedBigIconPath"),
    ];

    public static void Postfix(PowerModel __instance, ref string __result)
    {
        if (!HostShapeGate.Active || __instance is not RevertDiamondDiademPower)
        {
            return;
        }

        __result = OfficialBigIconPath;
    }
}
