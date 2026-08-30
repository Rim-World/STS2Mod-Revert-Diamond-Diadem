using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;
using STS2RitsuLib.Patching.Models;

namespace RevertDiamondDiadem.Patches;

/// <summary>
/// 减伤核心，复刻 107 DiamondDiademPower.ModifyDamageMultiplicative 的数值语义：
/// 持有 RevertDiamondDiademPower 的生物受到的普通攻击伤害减半（IsPoweredAttack = 带 Move 且不带 Unpowered）；
/// 其余伤害路径（毒、HP 流失、遗物/药水直伤等）不受影响。
///
/// 该虚方法在 107 为 5 参、111 为 6 参（末尾追加 CardPlay? cardPlay），单一 C# override 无法横跨两版，
/// 故以两条精确签名目标声明之：命中其一即可，另一条按 optional 忽略；postfix 参数按形参名注入，两个签名共用一份逻辑。
/// </summary>
public sealed class HalfDamagePatch : IPatchMethod
{
    public static string PatchId => "reddia_half_damage";
    public static string Description => "Halve powered-attack damage taken by the holder of the legacy buff";
    public static bool IsCritical => true;

    private const decimal DamageMultiplier = 0.5m;

    public static ModPatchTarget[] GetTargets() =>
    [
        PatchTarget.OptionalMethod(typeof(AbstractModel), "ModifyDamageMultiplicative",
            typeof(Creature), typeof(decimal), typeof(ValueProp), typeof(Creature), typeof(CardModel)),
        PatchTarget.OptionalMethod(typeof(AbstractModel), "ModifyDamageMultiplicative",
            typeof(Creature), typeof(decimal), typeof(ValueProp), typeof(Creature), typeof(CardModel), typeof(CardPlay)),
    ];

    // target/props 是原方法形参名，Harmony 按名注入；__result 为返回值
    public static void Postfix(AbstractModel __instance, Creature? target, ValueProp props, ref decimal __result)
    {
        if (!HostShapeGate.Active || __instance is not Models.RevertDiamondDiademPower power)
        {
            return;
        }

        if (target == null || target != power.Owner)
        {
            return;
        }

        if (!props.IsPoweredAttack())
        {
            return;
        }

        __result *= DamageMultiplier;
    }
}
