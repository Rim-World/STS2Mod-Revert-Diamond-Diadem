using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;

namespace RevertDiamondDiadem.Models;

/// <summary>
/// 107 版官方专属 buff 的复刻（原类 MegaCrit.Sts2.Core.Models.Powers.DiamondDiademPower 在 beta 111 已删除）。
/// 类名带 Revert 前缀避免与未来官方复活的原类撞 ModelId；本地化键随 slug 自动推导为
/// REVERT_DIAMOND_DIADEM_POWER.*，由本 mod pck 的 powers 表提供（内容照抄 v0.107 官方文案）。
///
/// 减伤数值不在本类实现：ModifyDamageMultiplicative 在 107/111 签名不同（5参→6参），
/// 由 HalfDamagePatch 按参数名注入的 Harmony postfix 承载，避免跨版本 override 失效。
///
/// 生命周期照抄 107 官方：AfterSideTurnEnd(side==Enemy) 时移除——即只覆盖施加后的那个敌方回合，
/// 敌方回合结束立即消失（漏写此 override 会导致 buff 永久常驻，2026-08-28 实测踩坑）。
/// 图标无需处理：官方图集条目与 powers 大图在 107/111 的资源包中均未删除（111 实测仍在），
/// 按 slug 推导的官方路径原生可显示原版图形，任何自定义覆盖反而加载失败（mod pck 内原始 png 无 remap）。
/// </summary>
public sealed class RevertDiamondDiademPower : PowerModel
{
    public override PowerType Type => PowerType.Buff;

    public override PowerStackType StackType => PowerStackType.Single;

    public override async Task AfterSideTurnEnd(
        PlayerChoiceContext choiceContext, CombatSide side, IEnumerable<Creature> participants)
    {
        if (side == CombatSide.Enemy)
        {
            await PowerCmd.Remove(this);
        }
    }
}
