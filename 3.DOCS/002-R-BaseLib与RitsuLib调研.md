# 第三方基础库调研：BaseLib-StS2 与 STS2-RitsuLib

> 调研日期：2026-08-27。远程数据（GitHub/Steam/NuGet API）为当日实时抓取；源码细节来自本地副本只读阅读：
> - `C:/Users/13167/Desktop/Mod/STS2-Reference/BaseLib-StS2/`（快照 v3.4.0，上游已到 v3.4.5）
> - `C:/Users/13167/Desktop/Mod/STS2-Reference/STS2-RitsuLib/`（快照 v0.5.10，上游已到 v0.5.15）
>
> 另可参照同目录既有的两份早期调研（达弗修改项目产出）：`C:/Users/13167/Desktop/Mod/STS2Mod-达弗修改/research/baselib-research.md`、`research/ritsulib-research.md`。

## 一、BaseLib

- **定位**：官方 repo 简介即 “standardize content additions”——把“新增内容”标准化的基础 mod。
- **作者/许可证**：Alchyr 等 37 位贡献者（747 commits 主导）；MIT。
- **活跃度**：repo 最近 push 2026-08-19；最新 release v3.4.5（2026-08-14）；有双分支（main/beta）适配史（v3.3.x~v3.4.x release notes 反复出现 “Beta compat”）。
- **生态规模**：Steam 创意工坊 item 3737335127 订阅约 **67.9 万**、NuGet `Alchyr.Sts2.BaseLib` 下载 2.85 万；配套模板包 `Alchyr.Sts2.Templates`、分析器包 `Alchyr.Sts2.ModAnalyzers` 各约 1.7 万下载。
- **入门手册**：
  - Wiki：<https://alchyr.github.io/BaseLib-Wiki/>（覆盖 custom-card/relic/character/orb/event/act/encounter/ancient/singleton/temporary-power 十类教程 + 本地化五篇 + 场景与工具类若干）；
  - Setup 手册在 ModTemplate 仓库 README / wiki（MegaDot 与 .NET SDK 安装、Workshop 订阅 BaseLib、`dotnet new install Alchyr.Sts2.Templates` 起项目、analyzer 生成本地化、Publish 打 pck 流程）。
- **功能面**（本地源码目录佐证）：43 个 Custom*Model 内容基类与 Pool；Hooks 扩展点（IAfterCardDowngraded、血条预测、手牌上限、scry 等）；12 个 Patches 子域（含存档扩展 ExtendedSavePatches、卡堆性能重写 TheBigPatchToCardPileCmdAdd）；9 个自定义动态变量类；游戏内设置页 UI 全套控件；FmodAudio 直播音频工具；BaseLibScenes 场景节点库；SpireField（给既有类外挂字段）、AddedNode、ModInterop 等。
- **运行时形态**：本身就是一个符合官方 manifest 的普通 mod（dll+pck+json），依赖为空数组；玩家需先装它（Workshop 订阅即可）。

## 二、STS2-RitsuLib

- **定位**：“A shared framework library”，README.zh 自述“不替代游戏原生 API，也不要求放弃 BaseLib……把常见 Mod 编写流程整理好的工程工具层”。
- **作者/许可证**：BAKAOLC（OLC）——**2005/2008 commits 实质单人项目**；MIT；致谢名单含 Alchyr、拂云FuYn 等。
- **活跃度**：极活跃——pushed 2026-08-26（本次调研前一天），8 月单月发 9 个正式版；dev-build prerelease 随每次 push 由 GitHub Actions 自动产出。
- **生态规模**：GitHub stars 168；NuGet 主包 `STS2.RitsuLib` 2.68 万下载 + Compat 变体家族合计约 1.6 万；Steam 订阅数未公开（workshop id 未硬编码）。
- **入门手册**：双语文档站 <https://sts2-ritsulib.ritsukage.com/guide/getting-started>（与本地 `docs/pages/guide/` 26 篇同步），覆盖 NuGet 引入、manifest dependencies 写法、最小骨架（`[ModInitializer]` → CreateLogger → RegisterModAssembly → 必要补丁失败自动禁用）、注解式注册与 content pack 流式 API、本地化键约定与占位符渲染。
- **功能面**（本地 src 1205 个 .cs 快照）：Scaffolding（卡/遗物/药水/角色/事件/timeline 注册 DSL）、Settings（schema 化玩家设置页）、Networking（联机状态同步/授权/分歧报告）、Combat hooks（AttackHitHook、目标类型、血条预测等）、Diagnostics（诊断日志+独立日志查看器 UI）、Audio（FMOD 深度桥接）、CardPiles/TopBar UI、Telemetry（opt-in 遥测）、Patching 辅助（带诊断的 Patcher）、持久化与跑局存档迁移等；schemas/ 目录有版本化的 JSON Schema 契约。
- **最独特的机制 —— variant-pack 多版本自适载**：一个安装文件夹内含 loader 与 `lib/<api-version>/` 多套构建；loader（`components/loader/Bootstrap.cs`，[ModInitializer] 入口）读 `ritsulib-variants.manifest`，按宿主版本 `Sts2HostVersion.Numeric` 选择 ≤ 宿主的最高变体 DLL 载入 AssemblyLoadContext 并逐 DLL SHA256 校验，再通过 ReflectionBridge 让官方 type discovery 认得变体程序集并登记进官方 `ModManager.AssociateAssemblyWithMod`。**这正是“跨版本单包发布”问题的成熟解**。
- **兼容矩阵显式声明**：`build/RitsuLib.CompatDefines.targets:20-21` 支持集 `0.103.2; 0.106.1; 0.107.0; 0.107.1; 0.108.0; 0.109.0; 0.110.0`（累计符号 `STS2_AT_LEAST_*` 条件编译），v0.5.12 起 "+compatibility with 0.111.0"。
- **运行时形态**：DLL-only（has_pck:false）；互相不依赖且对 BaseLib 有**反射式软桥接**（`BaseLibTargetTypeBridge.cs` 等，检测到 BaseLib 就桥接其自定义目标类型/手牌上限/血条预测功能，没有则降级自身实现）→ 两库可共存互补。

## 三、官方生态背景（两者共同的落脚点）

- 游戏原生支持 mod：manifest JSON 字段全集定义于反编译 `MegaCrit.Sts2.Core.Modding/ModManifest.cs`；`[ModInitializer]` 无标注则自动 `Harmony.PatchAll`；Harmony 运行库随本体分发（`data_sts2_windows_x86_64/0Harmony.dll`）。
- 官方构建底座是 Mega Crit 定制版 Godot「MegaDot」(<https://megadot.megacrit.com/>)；分发主渠道是 Steam Workshop；官方上传器 megacrit/sts2-mod-uploader（90 stars）。
- 本机已有上传器源码副本：`C:/Users/13167/Desktop/Mod/STS2-Reference/sts2-mod-uploader/`。
- NuGet 上以引用程序集/公共化为代表的第三方生态（MinionLib 双适配包、Publicizer 参考包等）印证“官方路线之上的标准件”定位。提示词里的 “Thunderspire” 未能在 GitHub/NuGet/域名中验证存在，当前可验证主渠道仅 Steam Workshop + GitHub Releases/NuGet。

## 四、依赖 vs 不依赖：利弊要素表

| 维度 | BaseLib | RitsuLib | 不依赖任何一方 |
|---|---|---|---|
| 玩家额外负担 | 先装 1 个 base mod（67.9 万订阅，几乎零教育成本） | 先装 1 个 DLL-only base mod | 无 |
| 开发接入成本 | 模板一键起项目；英文 wiki 为主；analyzer 生成 loc | 注解 DSL/流式注册；中文双语文档站 26 页 | 全部样板自写（本项目场景下样板很少，见 §五） |
| 能力上限 | 新增内容基类+设置页+SpireField 等 | 更广（联机/遥测/schema/settings/variant 加载） | 无限制 |
| 对“修改 vanilla 行为”的支持 | 无直接 API（注册新内容导向，vanilla 改动仍须自己 Harmony） | 同左（有补丁脚手架但非封装层） | 同样需要自己 Harmony，量级相同 |
| 版本联动风险 | 高频小版本无 semver；曾为 beta 分支临时停用 patch | 0.x 周更级节奏；deprecation 有兼容窗口；compat 矩阵显式锁定旧版本对旧目标友好 | 独立演进；每次游戏更新自己扛，但本项目防波堤已内置（形状守卫） |
| 维护结构 | 单核心作者+37 贡献者 | **实质单人**（99.75% commits）；CI/签名/发布自动化完善 | 不受第三方生命线影响 |

## 五、对本项目（钻石头冠回调）的适配性分析

本项目的需求特征：

1. 核心 = 修改一个 vanilla 遗物 + 补回一个 vanilla 已删除的专属 buff 的行为 → 两库都只有“新增内容”API，这条主干工作无论如何都要自己写 Harmony；
2. 需要“mod 程序集中的新 PowerModel 子类能被引擎发现” → **已实证官方原生支持**：`ModelDb.AllAbstractModelSubtypes` 显式合并 `ReflectionHelper.GetSubtypesInMods<AbstractModel>()`（见 3.DOCS/001-E-钻石头冠版本差异取证.md 与 `3.DOCS/003-T-STS2-Modding机制事实速查.md`），并不需要库的注册体系来代劳；
3. 计数器 UI 直接复用 RelicModel 原生 Status/DisplayAmount/Flash 机制；
4. 无音频、无场景扩展、无联机、无遥测、无 schema 化设置页需求；
5. 文本交付用 mod 自己 pck 的 localization/<lang>/powers.json + relics.json 即可；
6. 跨版本问题中真正麻烦的一处（ModifyDamageMultiplicative 5参→6参）计划用“条件挂载双 postfix”解决（ADR-001），不需要 RitsuLib 的 variant-pack 那种整套机甲。

结论性对照（供拍板，最终取舍由人决定）：

- **依赖的收益**在本项目中具体只剩两条：a) 设置页/存档轮子——不需要；b) 若未来想放弃“条件挂载”改为双变体 DLL 发布，RitsuLib variant-pack 是现成方案——但那是过度设计，本项目一个 postfix 二选一即可。
- **依赖的成本**：两个 20 万量级订阅之外的玩家还要多装一个前置；构建链加一个 NuGet 解析环节；“回调效果必须精确复刻”的场景里引入库行为面反而增加排查变量；两库自身的 breaking 风险会成为 mod 的上游风险。
- **两份既有先例一致佐证**：回调系全部 mod（同步/王座/飞拳/至亮之焰/潦草急就…）均零依赖完成同类工作；达弗修改用了 RitsuLib 是因为其功能是“扩池子+设置页”（内容型，库的价值大头）。
