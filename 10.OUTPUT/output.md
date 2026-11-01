# output — 构建产物与发布包索引

本目录存放构建产物与发布包。除本索引外，全部内容被根 `.gitignore` 排除，不入库。

| 时间 | 名称 | 版本号 | 大小 | 说明 |
|---|---|---|---|---|
| 2026-11-01 | `02-发布包/RevertDiamondDiadem.v1.0.0.zip` | 1.0.0 | 103,979 B | 安装包。从游戏目录 `mods/RevertDiamondDiadem/` 打包，内含 `RevertDiamondDiadem.dll`、`.pck`、`.pdb` 与 manifest；已排除以 `.` 开头的目录（`.sts2-workshop-uploader` 含本地上传状态） |
| 2026-11-01 | `02-发布包/RevertDiamondDiademSourceCode.zip` | 1.0.0 | 88,090 B | 源码包。从 `9.SRC/RevertDiamondDiadem/` 打包，已排除 `.godot/`、`bin/`、`obj/`、`*.dll`、`*.pck`、`*.pdb` |

## 目录约定

| 目录 | 用途 |
|---|---|
| `01-构建产物/` | 本地构建中间产物与调试输出（当前为空：`build.ps1` 直接把三件套写入游戏 `mods/`） |
| `02-发布包/` | 对外分发的压缩包 |

## 说明

- 这两份压缩包即 Release `v1.0.0` 的两个资产，可通过 `gh release create` 直接引用本地路径上传。
- 重新打包后请同步更新本表的文件大小与时间。
