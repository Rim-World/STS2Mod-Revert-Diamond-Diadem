# STS2Mod-回调头冠（RevertDiamondDiadem）

把《杀戮尖塔 2》先古遗物「钻石头冠」（Diamond Diadem）在 beta 111 上的新效果回调为 v0.107 的旧效果。

Reverts the effect of the Ancient relic "Diamond Diadem" from the v0.111 behavior back to the pre-v0.109 effect, patched in place on the original relic.

- 当前版本：`1.0.0`
- 运行平台：PC（Windows），《杀戮尖塔 2》`0.109.0` 及以上
- 前置依赖：`STS2-RitsuLib` ≥ `0.5.15`
- 开源协议：MIT

## 功能

| 版本 | 官方效果 |
|---|---|
| default 107（本 mod 恢复的目标） | 每个玩家回合结束时结算：若该回合打出 ≤2 张牌，获得专属 buff「受普通攻击伤害减半」，持续到下个敌方回合结束；遗物旁有出牌计数器 |
| beta 111（官方现行为） | 战斗开始时一次性获得 20 点格挡 + 1 层残影（格挡保留） |

本 mod 直接修改原遗物的行为，不新建遗物、不禁用旧遗物，具体包括：

- 短路 111 版的开局格挡与残影触发，并摘除随之新增的格挡悬停提示；
- 恢复出牌计数、计数数字显示与「≤2 张时高亮」；
- 回合结束时按旧版判定施加专属减伤 buff，敌方回合结束即移除；
- 遗物描述改读 v0.107 官方文案，提供 16 种语言；
- 减伤 buff 的名称、图标与文本均指向 v0.107 官方资源，mod 不自带图片。

新旧两版各有优劣：新版在道中的止损能力更强，旧版面对三层 Boss 的表现更好。本 mod 面向偏好旧版设计感的玩家。

## 兼容性

- **游戏版本**：manifest 声明 `min_game_version` = `0.109.0`，更早版本由游戏侧拒绝加载。
- **形态自适应**：启动时用两个反射判据判断当前游戏的钻石头冠实现形态——遗物本体是否自己声明了 `AfterSideTurnStart`、官方 `DiamondDiademPower` 类型是否已缺失。只有「新效果形态」才注册补丁；在旧效果宿主上整个 mod 为零补丁 no-op，不报错、不崩溃。
- **失败自禁用**：必需补丁应用失败时整个 mod 自动停用，行为回到原版。
- **不保证**：盗版、修改版、移动版、过旧版本，以及冷门或次级 mod 的组合兼容性。
- **存档影响**：开启本 mod 时，历史记录中的钻石头冠描述按回调文案显示；关闭后恢复原状。计数状态不入存档。

## 安装

1. 先安装前置库 `STS2-RitsuLib`（≥ `0.5.15`）；
2. 将发布包解压到 `Slay the Spire 2/mods/RevertDiamondDiadem/`，目录内应有 `RevertDiamondDiadem.dll`、`RevertDiamondDiadem.pck` 与 `RevertDiamondDiadem.json` 三个文件；
3. 启动游戏，日志出现 `initialized; diamond diadem reverted to legacy (v0.107) behavior.` 即为生效。

在旧效果版本上日志会给出 `host already has the legacy (v0.107) behavior; nothing to do, staying no-op.`，属预期行为。

## 构建

源码位于 `9.SRC/RevertDiamondDiadem/`，构建依赖 .NET 9 SDK、Godot 4.5.1 mono 与本机游戏安装。

```powershell
cd 9.SRC/RevertDiamondDiadem
pwsh ./build.ps1
```

`build.ps1` 依次执行：`dotnet build` → `godot --headless --import` → `godot --headless --export-pack "Windows Desktop"` → 把 dll、pdb、manifest、pck 拷入游戏 `mods/RevertDiamondDiadem/`。脚本内的 `$game` 与 `$godot` 为开发者本机绝对路径，需按自己的环境修改。

工程引用游戏目录下的 `sts2.dll` 与 `0Harmony.dll`（`Private=false`，不随包分发），并通过 `Krafs.Publicizer` 公开化 `sts2` 以便访问受保护成员。

## 目录结构

```
STS2Mod-回调头冠/
├── 1.ADR/         架构决策记录（ADR.md 为索引）
├── 2.PLANS/       施工计划（PLANS.md 为索引）
├── 3.DOCS/        知识与报告（DOCS.md 为索引）
├── 4.SDD/         软件设计文档（SDD.md 为索引）
├── 5.ChangeLog/   版本日志分片（ChangeLog.md 为索引）
├── 9.SRC/         源码（RevertDiamondDiadem/）
├── 10.OUTPUT/     构建产物与发布包（output.md 为索引，内容不入库）
├── PRIVACY/       机密与隐私（不入库）
├── TMP/           临时与过程文件（TMP.md 为索引，内容不入库）
├── TOOLS/         工具与自写脚本（TOOLS.md 为索引）
├── description/   创意工坊/发布文案（中英文）
└── pics/          封面与素材图
```

源码内部结构保持不变，其面向玩家的中英文说明见 `9.SRC/RevertDiamondDiadem/README.zh.md` 与 `9.SRC/RevertDiamondDiadem/README.md`。

## 文档索引

- 技术路线决策：[1.ADR/001-钻石头冠回调技术路线选型.md](1.ADR/001-钻石头冠回调技术路线选型.md)（索引：[1.ADR/ADR.md](1.ADR/ADR.md)）
- 施工计划：[2.PLANS/001-G-钻石头冠效果回调.md](2.PLANS/001-G-钻石头冠效果回调.md)（索引：[2.PLANS/PLANS.md](2.PLANS/PLANS.md)）
- 总体设计：[4.SDD/001-总体设计.md](4.SDD/001-总体设计.md)（索引：[4.SDD/SDD.md](4.SDD/SDD.md)）
- 版本差异取证：[3.DOCS/001-E-钻石头冠版本差异取证.md](3.DOCS/001-E-钻石头冠版本差异取证.md)
- BaseLib / RitsuLib 调研：[3.DOCS/002-R-BaseLib与RitsuLib调研.md](3.DOCS/002-R-BaseLib与RitsuLib调研.md)
- Modding 机制事实速查：[3.DOCS/003-T-STS2-Modding机制事实速查.md](3.DOCS/003-T-STS2-Modding机制事实速查.md)
- 图标解包记录：[3.DOCS/004-T-PCK内BC7纹理解包记录.md](3.DOCS/004-T-PCK内BC7纹理解包记录.md)
- 文档总索引：[3.DOCS/DOCS.md](3.DOCS/DOCS.md)
- 版本日志：[CHANGELOG.md](CHANGELOG.md)、[5.ChangeLog/ChangeLog.md](5.ChangeLog/ChangeLog.md)

## 版本

当前发布版本 `1.0.0`，发布于 2026-11-01。版本变更见 [CHANGELOG.md](CHANGELOG.md)。

## 致谢

- 感谢《杀戮尖塔 2》制作组为玩家制作 mod 所做的努力；
- 感谢 RitsuLib 制作者提供的开源框架与文档；
- 本 mod 的调研与实现过程中使用了大语言模型辅助，工具署名与评价保留在 `description/回调头冠_中文.md` §⑥。

## 开源地址

<https://github.com/Rim-World/STS2Mod-Revert-Diamond-Diadem>

问题反馈与建议可在仓库 Issue 区或创意工坊评论区提出。
