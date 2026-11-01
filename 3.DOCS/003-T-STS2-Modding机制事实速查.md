# STS2 Modding 机制事实与本机工程惯例速查

> 目的：把散落在各既有项目 docs/adr 与反编译取证里的、本 mod 会反复用到的机制事实集中沉淀，避免每个新项目重复考古。
> 来源均为本机绝对路径可直接复核；调研日期 2026-08-27。

## 一、加载链（已实证）

1. manifest `mods/<ModId>/<ModId>.json` 字段全集：id/name/author/description/version/has_pck/has_dll/min_game_version/dependencies/affects_gameplay —— 反编译定义 `C:/Users/13167/Desktop/Mod/STS2-Reference/decompiled-111/MegaCrit.Sts2.Core.Modding/ModManifest.cs`。
2. 入口约定：`[ModInitializer("方法名")]` 标注静态类方法；若无该特性则自动 `Harmony.PatchAll(mod 程序集)`。注释原文见 `decompiled-111/MegaCrit.Sts2.Core.Modding/ModInitializerAttribute.cs`。
3. Harmony 运行库由游戏本体携带：`C:/Game/Steam/steamapps/common/Slay the Spire 2/data_sts2_windows_x86_64/0Harmony.dll` → csproj 以 `Private=false` 引用、不随包分发。
4. 本地化按约定探测：`ModManager.GetModdedLocTables(language, file)` 逐个 loaded mod 检查 `res://{manifest.id}/localization/{language}/{file}` 是否存在（`decompiled-111/MegaCrit.Sts2.Core.Modding/ModManager.cs:966`）。三字符语言码作目录名；缺失语言目录=该语言不加载该表；缺键会抛 LocException。
5. 模组图标从 pck 读：`res://{id}/mod_image.png`（420×420），即使无其他资源也要 has_pck=true（杂耍计数 ADR-002 已实证）。

## 二、ModelDb / 自定义模型发现（本次新增实证）

- `MegaCrit.Sts2.Core.Models.ModelDb`：

```csharp
/// Prefer this over using AbstractModelSubtypes.All because it also returns models in mods.
public static Type[] AllAbstractModelSubtypes
{
    // AbstractModelSubtypes.All (游戏本体) + ReflectionHelper.GetSubtypesInMods<AbstractModel>() (mod)
}
```
位置：`decompiled-111/MegaCrit.Sts2.Core.Models/ModelDb.cs:66-87`。

- `ReflectionHelper.GetSubtypesInMods(parentType)`：来源为 `ModManager.GetLoadedMods().SelectMany(m => m.assemblies).SelectMany(a => a.GetTypes())` 中非抽象的子类；**在 `ModManagerState.None` 时抛异常**（“ReflectionHelper.ModTypes cannot be called before that.”），`decompiled-111/MegaCrit.Sts2.Core.Helpers/ReflectionHelper.cs:38-66`。
- 推论：mod DLL 里直接新建 `class XxxPower : PowerModel {}` 即可被引擎收录；但**初始化早期不能触发这条查询**——与“回调飞拳” ADR-002 的 ModelDb 访问时机教训同源：`ModLoaded` 里禁止访问 ModelDb/LocManager，相关检查一律延迟到补丁首次触发时懒评估。
- ID 推导：`StringHelper.Slugify(Type.Name)`（PascalCase→大写下划线）；重名 slug 的模型会撞 ModelId（自定义类命名时要避开官方已有类名）。
- Power 实例化路径：`PowerCmd.Apply<T>(...)` → `ModelDb.Power<T>()` → 走 AllAbstractModelSubtypes，自定义 power 可直接泛型施加（`decompiled-111/MegaCrit.Sts2.Core.Commands/PowerCmd.cs:71`）。

## 三、跨版本兼容已知矩阵（截至 2026-08-27）

| 项目 | 结论 | 来源 |
|---|---|---|
| sts2.dll AssemblyVersion | 固定 `0.1.0.0` 不随版本变 → 绑定无障碍 | 杂耍计数 docs/compat-107-report.md |
| 编译目标选择原则 | 以“成员最少”的旧版（107）sts2.dll 为编译目标 → memberref 只引共有面，双版可解析；反向不可（111 新成员 107 没有） | 同上 |
| 四个战斗生命周期虚方法签名 | `AfterSideTurnStart/AfterSideTurnEnd/BeforeSideTurnEnd/AfterCardPlayed` 在 RelicModel 声明处两版逐字一致 | 本次 grep 双树验证（§六） |
| `AbstractModel.ModifyDamageMultiplicative` | **漂移**：107 五参 / 111 六参(+CardPlay?) | `AbstractModel.cs` 107:1580 vs 111:1613 |
| `CardCmd.Exhaust` 返回类型 | 110 起 `Task` → `Task<CardPileAddResult?>` | STS2-Reference/v110-to-v111-compat.md |
| `CardPlay.Player` 属性 | 107 无（统计谓词须用 `e.CardPlay.Card.Owner.Creature`） | 杂耍计数 ADR-003 |
| 语言表数量 | **107 官方为 14 语种**（filelist.txt 实证：无 ind、zht；含 tha）。注意两点：① `1.STS2-107Reference/localization/` 解包目录里混有从 111 复制的 ind/zht 文件（MD5 与 111 解包一致），读参考数据须以 filelist 白名单过滤；② 107 官方泰语翻译不完整——`tha/powers.json` 缺 `DIAMOND_DIADEM_POWER.title` 键（游戏内官方行为即回落 eng 表）。110/111=16（110 加 zht、111 加 ind） | 本项目 filelist 取证 + 提取脚本实跑 |
| 版本判定 API | `ReleaseInfoManager.Instance.SemVer`（Major/Minor/Patch），懒求值+缓存；不要用 loader 的 PlatformBranch 区分 110/111（同属 beta） | 回调同步 ModConfig.cs、飞拳 ADR-001 |
| Steam 自动升级 | 会悄悄覆盖降级产物；以反编译当前 sts2.dll 为准而非 release_info.json | 飞拳 ADR-003 |

## 四、蓝图补丁四件套（回调系惯例）

1. **版本门**（ModConfig.cs）：SemVer 判定 + 缓存 + 未就绪暂放行；只锁 Minor 区间，“加一少改多”。
2. **形状守卫**（XxxShapeGuard.cs）：启动时用 `AccessTools.TypeByName/Method/PropertyGetter` 确认全部补丁目标存在，缺则整体禁用 + Log.Warn。
3. **条件挂载**（PatchIfExists）：版本特有成员存在才 `_harmony.Patch(...)`，两版差异区域逐成员处理。
4. **两级防炸**：静态守卫一次 + 补丁体内对 ModelDb/Card 实例再查一次（首次触发时懒评估），任一失败走放行原实现或禁用路径，绝不抛异常炸游戏主循环。

虚分派陷阱提醒（回调同步 ADR-001 修订记录）：patch “基类虚方法声明处”时若派生类 override 了它会被绕过；patch “派生类方法名”时若该成员实际声明在基类则 Harmony 定位失败。故每个补丁都要先确认 DeclaringType。本项目已给出判据：DiamondDiadem 上 `AfterSideTurnStart` 的 DeclaringType 是不是 DiamondDiadem 本身 = 当前是否为“新效果版本”。

## 五、交付物形态与构建（照抄即可）

产物 4 文件入 `mods/<ModId>/`：`<ModId>.dll`、`<ModId>.pck`、`<ModId>.json`、（可选）`.pdb`。

build.ps1 三步流水线（模板：`C:/Users/13167/Desktop/Mod/STS2Mod-回调同步/src/RevertSynchronize/build.ps1`）：

```powershell
dotnet build <ModId>.csproj -c Debug
& <GodotMonoConsole> --headless --import --path .
& <GodotMonoConsole> --headless --export-pack "Windows Desktop" <输出pck> --path .   # 直接落游戏 mods 目录
Copy-Item ... dll/pdb/json → mods/<ModId>/
```

csproj 要点（模板：回调同步 / 达弗修改）：`Godot.NET.Sdk/4.5.1`、net9.0、引用 `$(Sts2DataDir)/sts2.dll` 与 `0Harmony.dll`（Private=false）、需要私有成员访问时加 Krafs.Publicizer `<Publicize Include="sts2"/>`、MSB3270 抑制。

中间文件策略（杂耍计数 ADR-002）：命令行 `-p:` 把 obj/bin/.godot 缓存重定向到项目内 `%TEMP%/build`、`%TEMP%/godot-cache`（注意 bash 下 `%` 要写成 `%25`——PowerShell 构建脚本里无此问题）。

文本提取脚本模板：`STS2Mod-回调同步/script/extract-synchronize-localization.py`（纯标准库解析 GDPC pck、提取官方键并生成 mod 各语言 json）。

## 六、本项目已核对的钩子/成员原始记录

以下四个虚方法声明在两版反编译树中 grep 出的原文（逐字一致，证明可安全作为单 DLL 的 Harmony target 或 C# override 面）：

```
(107 & 111 both, declared on RelicModel/AbstractModel chain):
public virtual Task AfterSideTurnStart(CombatSide side, IReadOnlyList<Creature> participants, ICombatState combatState)
public virtual Task AfterSideTurnEnd(PlayerChoiceContext choiceContext, CombatSide side, IEnumerable<Creature> participants)
public virtual Task BeforeSideTurnEnd(PlayerChoiceContext choiceContext, CombatSide side, IEnumerable<Creature> participants)
public virtual Task AfterCardPlayed(PlayerChoiceContext choiceContext, CardPlay cardPlay)
```

RelicModel 关键 UI 成员可见性（111，107 同构）：`Status` public getter+setter（带 AssertMutable，`RelicModel.cs:330`）、`ShowCounter => false` virtual（347）、`DisplayAmount => 0` virtual（349）、`DisplayAmountChanged`/`StatusChanged` event（426/428）、`InvokeDisplayAmountChanged()` protected（468）、`Flash()`（458）、`CanonicalVars` protected virtual（302）。公开 setter 意味着多数场景无需 Publicizer。

PowerModel 抽象成员全集仅两个：`Type`（142 行）、`StackType`（236 行）——最小 custom power 子类的实现成本极低。

`GetModdedLocTables` 只是枚举候选表路径，官方与 mod 表的最终合并优先级未逐行验证；本系列惯例采用 RE_ 独立键 + getter postfix 改读取路径来回避竞争问题（飞拳 ADR-001 决策 2），本项目沿用此决策即可，无需深究合并顺序。

## 七、已知风险点清单（施工期逐条打勾）

- [ ] `ValuePropExtensions.IsPoweredAttack()` 两版签名核对
- [ ] `Hook.ModifyDamage*` 静态广播器两版结构核对（决定减伤 postfix 的最稳 patch 点）
- [ ] Creature 侧 power 查询 API（如 HasPower/GetPower<T>）两版共有性核对
- [ ] PowerModel 图标资源关联方式（Icon 属性推导路径）与自定义 power 图标放置位置
- [ ] `DynamicVarSet` 构造时序（补丁 CanonicalVars postfix 是否赶得上 ctor 组装；否则改为注入 DynamicVarSet）
- [ ] DiamondDiadem 在存档缓存中的序列化行为（_cardsPlayedThisTurn 曾进 107 存档与否——111 上我们自己维护计数，避免触碰原字段）

## 八、二轮实测沉淀的新事实（2026-08-28）

1. **mod pck 内的原始 png 运行时不可加载**：`ResourceLoader.Load("res://<modid>/.../x.png")` 报 "No loader found for resource"——pck 内原始 png 没有 .import remap 可用（即使 export 打进了 .import 与 .ctex，`res://` 合并空间下 remap 解析也不生效）。mod 可靠的图片通道只有 `res://{id}/mod_image.png`（引擎 mod 管理专用）。
2. **官方删除内容代码时不清理美术资源**：111 中 `DiamondDiademPower` 类已删，但其图集条目 `images/atlases/power_atlas.sprites/diamond_diadem_power.tres`、`images/powers/diamond_diadem_power.png.import` 与 imported ctex 三件俱全——回改 mod 可直接把自定义 power 的图标路径 patch 回官方原类资源路径。**注意 slug 陷阱**：图标路径按“类型名 slug”推导，mod 自定义类名带前缀（如 RevertDiamondDiademPower→revert_diamond_diadem_power）时路径必然对不上官方条目，原生显示必为 missing 占位图；必须显式 patch 指回官方原类资源路径（107/111 均实证可加载）。
3. **复刻官方 power 类时逐成员核对生命周期钩子**：本例漏抄 `AfterSideTurnEnd(side==Enemy) => PowerCmd.Remove(this)` 导致 buff 永久常驻。107 原版语义：玩家回合末施加 → 敌方回合减伤 → 敌方回合末移除。
4. 头冠 tres 的 HUD 小图标为 `AtlasTexture`（region=Rect2(706,529,64,62)，margin=Rect2(0,0,0,2)），引用 `power_atlas.png`；`pck-tool.py`（本项目 `TOOLS/scripts/`）可对任意游戏 pck 做目录搜索与定点提取。
