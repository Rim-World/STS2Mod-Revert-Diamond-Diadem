# TMP — 临时与过程文件索引

本目录存放施工期产生的临时文件、缓存与旧版仓库快照。除本索引外，全部内容被根 `.gitignore` 排除，不入库、不打包、不上传。

## 一级分类

| 编号 | 目录 | 说明 |
|---|---|---|
| 01 | `01-旧Git仓库/` | 整理前的嵌套克隆仓库快照，仅供追溯，不再参与任何操作 |
| 02 | `02-缓存与临时/` | 图标解包、纹理转换与 pck 校验的中间产物，可再生 |

## 文件清单

| 时间 | 文件/目录 | 用途 | 格式 | 大小 | 来源 |
|---|---|---|---|---|---|
| 2026-08-30 | `01-旧Git仓库/RevertDiamondDiadem-克隆.git/` | 整理前 `src/RevertDiamondDiadem/.git` 的完整快照（2 个提交） | git 仓库目录 | 128 KB | 原嵌套克隆，本地仅此一份 |
| 2026-08-28 | `02-缓存与临时/godot-crop/atlas.ctex` | 官方 `power_atlas.png` 的压缩纹理，用于裁切 HUD 小图标 | Godot ctex | 1.09 MB | pck-tool 从游戏 pck 提取 |
| 2026-08-28 | `02-缓存与临时/godot-crop/big.ctex` | 官方 `diamond_diadem_power.png` 的压缩纹理，用于 tooltip 大图 | Godot ctex | 64 KB | 同上 |
| 2026-08-28 | `02-缓存与临时/godot-crop/crop.gd` | 临时 Godot 脚本：ctex 转 png 并按 tres 的 region 裁切 | GDScript | 1.3 KB | 自写 |
| 2026-08-28 | `02-缓存与临时/godot-crop/project.godot` | 上述转换用的最小 Godot 工程配置 | Godot 工程 | 152 B | 自写 |
| 2026-08-28 | `02-缓存与临时/pck-extract/.godot/imported/` | 两个压缩纹理的导入缓存（s3tc / bptc） | ctex | 1.15 MB | pck-tool 提取 |
| 2026-08-28 | `02-缓存与临时/pck-extract/images/` | 图集条目 `.tres` 与 `diamond_diadem_power.png.import` | 文本 + 配置 | 523 B | pck-tool 提取 |
| 2026-08-29 | `02-缓存与临时/pck-verify/` | 从打包产物中回抽的 tha 语言表，用于核对入包内容 | JSON | 749 B | pck-tool 提取 |

## 清理建议

- `02-缓存与临时/` 全部为可再生中间产物，确认不再复查图标解包过程后可整目录清理；解包方法与结论已固化在 `3.DOCS/004-T-PCK内BC7纹理解包记录.md`，不依赖这些文件。
- `01-旧Git仓库/` 是整理前克隆的副本，历史已由远端仓库与根仓库的新提交承接；如需节省空间可在确认远端可访问后清理。
- 源码目录内的 `9.SRC/RevertDiamondDiadem/.godot/`（约 29 MB）是 Godot 导入缓存，已被源码目录自带的 `.gitignore` 排除；它不属于本目录，但同样可再生，如需瘦身可另行清理。

以上清理动作均需取得项目所有者明确同意后执行。
