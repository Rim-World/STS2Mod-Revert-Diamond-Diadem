# TOOLS — 工具与自写脚本索引

本目录只收纳两类内容：以工具命名的安装子目录，以及自写脚本统一目录 `scripts/`。本项目没有需要在项目内安装的第三方工具——构建依赖的 .NET SDK、Godot 4.5.1 mono 与本机游戏环境均为系统级安装，不在本目录内。

## 自写脚本（`scripts/`）

| 时间 | 工具/脚本 | 用途 | 版本 | 大小 | 来源 | 安装方式/依赖 |
|---|---|---|---|---|---|---|
| 2026-08-27 | `scripts/gen-blank-icon.py` | 生成 420×420 的占位 `mod_image.png`（纯色底 + 中央皇冠形状），正式宣传图就位后由美术图覆盖 | 未标注 | 1,924 B | 自写，方案沿用本 series 的占位图脚本 | Python 3 标准库（`zlib`、`struct`） |
| 2026-08-28 | `scripts/pck-tool.py` | Godot pck 目录搜索与定点提取：`search <pck> <子串>` 列条目，`extract <pck> <子串> <输出目录>` 按原相对路径写出 | 未标注 | 2,249 B | 自写；用于定位官方图标资源与校验打包产物入包内容 | Python 3 标准库（`struct`、`sys`），支持 GDPC v3 |
| 2026-08-29 | `scripts/extract-diadem-localization.py` | 从 v0.107 解包资源提取官方文本，生成 16 语言 `relics.json`/`powers.json`；zht 由 zhs 转写（源文漂移会报错），ind/tha 的缺项回落 eng | 未标注 | 5,764 B | 自写，参照本 series 同步项目的提取脚本改写 | Python 3 标准库；只读 `STS2-Reference` 下的解包资源 |

## 使用注意

- 两个生成类脚本（`gen-blank-icon.py`、`extract-diadem-localization.py`）的输出路径以脚本自身位置为基准、按整理前的 `src/RevertDiamondDiadem/` 布局推算。仓库整理到 `9.SRC/` 后脚本内容未作改动以保持代码原样，直接运行会指向不存在的 `TOOLS/src/...`；如需复用，请先按当前布局调整输出路径常量，或在临时副本中修改后再执行。
- `pck-tool.py` 的输入输出全部来自命令行参数，不受目录整理影响。
- 调用 `pck-tool.py` 提取游戏资源时，请从自己的游戏安装目录读取，提取产物属于过程文件，应放入 `TMP/02-缓存与临时/`。
