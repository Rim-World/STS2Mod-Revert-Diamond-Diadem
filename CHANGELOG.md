# Changelog

本文件遵循 [Keep a Changelog](https://keepachangelog.com/zh-CN/1.1.0/) 规范，版本号遵循 [语义化版本](https://semver.org/lang/zh-CN/)。

## 未发布

### 变更

- 仓库整理为 `1.ADR` / `2.PLANS` / `3.DOCS` / `4.SDD` / `5.ChangeLog` / `9.SRC` / `10.OUTPUT` / `PRIVACY` / `TMP` / `TOOLS` 十目录结构，源码整体迁入 `9.SRC/RevertDiamondDiadem/`；
- 文档按编号与分类重命名，新增总体设计、版本日志、项目 README、本更新日志与各目录索引；
- 仓库整理提交：`8dc2105`。

## [1.0.0] - 2026-11-01

首个正式版本。

### 新增

- 将「钻石头冠」在 beta 111 上的开局格挡 + 残影效果短路，改为 default 107 的回合结束结算逻辑：本回合打出 ≤2 张牌时施加「受普通攻击伤害减半」buff，持续到该次敌方回合结束；
- 自建减伤 buff `RevertDiamondDiademPower`，名称、图标与文本指向 v0.107 官方资源，敌方回合结束后自动移除；
- 恢复遗物出牌计数器：战斗内显示本回合已出牌数，≤2 张时遗物高亮；
- 遗物描述改读 v0.107 官方文案，覆盖 16 种语言（deu / eng / esp / fra / ind / ita / jpn / kor / pol / ptb / rus / spa / tha / tur / zhs / zht）；
- 摘除 111 版新增的格挡悬停提示，与减伤语义保持一致。

### 变更

- 启用判定采用行为级形状守卫（遗物是否自有 `AfterSideTurnStart` override + 官方 `DiamondDiademPower` 是否缺失），不使用版本号；旧效果宿主上整个 mod 为零补丁 no-op；
- manifest 最低游戏版本定为 `0.109.0`，前置依赖 `STS2-RitsuLib` ≥ `0.5.15`；
- 减伤逻辑以双签名 Optional 目标挂载在 `AbstractModel.ModifyDamageMultiplicative` 上，兼容 107 五参与 111 六参。

### 修复

- 计数清零后未刷新 UI，导致遗物旁数字停留在旧值；
- 复刻 buff 时漏写 `AfterSideTurnEnd` 移除逻辑，导致 buff 永久常驻；
- buff 图标回落为占位图：类名带 `Revert` 前缀导致 slug 推导路径不存在，改为补丁改写两个图标 getter 指向官方原类资源。

### 已知问题

- 开启 mod 时历史记录中的遗物描述会显示回调文案，关闭后恢复；这是设计行为，非缺陷。
- mod 详情、创意工坊文案与图片素材需与发布包手动同步。
