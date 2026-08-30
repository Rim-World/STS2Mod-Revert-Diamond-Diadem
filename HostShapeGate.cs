using System.Reflection;
using MegaCrit.Sts2.Core.Logging;

namespace RevertDiamondDiadem;

/// <summary>
/// 宿主形态守卫：只用 typeof/反射枚举判定当前游戏的钻石头冠实现形态，
/// 全程不触碰 ModelDb/LocManager（加载早期它们不可用，见回调飞拳 ADR-002 教训）。
///
/// 判据① 新效果版：DiamondDiadem 自身声明了 AfterSideTurnStart override（beta 111 起才有，default 107 无）；
/// 判据② 安全阀：官方专属 buff 类 DiamondDiademPower 尚未复活（防未来版本改回旧名造成 ModelId 双重注册冲突）。
/// ① && ② ⇒ 生效。所有补丁体内的 Active 快速检查是第二道保险。
/// </summary>
public static class HostShapeGate
{
    private const string LegacyPowerTypeName =
        "MegaCrit.Sts2.Core.Models.Powers.DiamondDiademPower";

    public static bool IsNewEffectVersion { get; private set; }
    public static bool Disabled { get; private set; }
    public static bool Active => IsNewEffectVersion && !Disabled;

    public static bool Evaluate(Logger log)
    {
        var relicType = Type.GetType(
            "MegaCrit.Sts2.Core.Models.Relics.DiamondDiadem, sts2", throwOnError: false)
            ?? AccessFindGameType("MegaCrit.Sts2.Core.Models.Relics.DiamondDiadem");

        if (relicType == null)
        {
            Disabled = true;
            log.Warn("DiamondDiadem type not found; disabling.");
            return false;
        }

        const BindingFlags declaredInstance =
            BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.DeclaredOnly;

        var declaresOwnAfterSideTurnStart = relicType
            .GetMethods(declaredInstance)
            .Any(m => m.Name == "AfterSideTurnStart");

        var officialLegacyPowerExists = AppDomain.CurrentDomain.GetAssemblies()
            .Any(a => a.GetType(LegacyPowerTypeName, throwOnError: false) != null);

        IsNewEffectVersion = declaresOwnAfterSideTurnStart && !officialLegacyPowerExists;

        log.Info($"host shape: own AfterSideTurnStart={declaresOwnAfterSideTurnStart}, "
                 + $"legacy power present={officialLegacyPowerExists}");

        if (!IsNewEffectVersion)
        {
            log.Info("host already has the legacy (v0.107) behavior; nothing to do, staying no-op.");
            return false;
        }

        return true;
    }

    public static void Disable()
    {
        Disabled = true;
    }

    private static Type? AccessFindGameType(string fullName)
    {
        foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
        {
            var type = assembly.GetType(fullName, throwOnError: false);
            if (type != null)
            {
                return type;
            }
        }

        return null;
    }
}
