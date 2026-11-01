# 钻石头冠图标解包记录（BC7 压缩纹理 ctex 解码法）

> 2026-08-29。产出 `pics/钻石头冠.png`（256×256 RGBA 透明背景）。
> 通用 pck format 3 结构见达弗项目 `STS2Mod-达弗修改\docs\pck-unpack-notes.md`，本文只记补充差异。

## 资源定位

- 中文名“钻石头冠”在 `localization/zhs/relics.json` 中对应遗物 ID **`DIAMOND_DIADEM`**（`DiamondDiadem` 类）。
- pck 内相关文件：
  - 遗物图标：`images/relics/diamond_diadem.png` → 导入产物 `.godot/imported/diamond_diadem.png-ada0f42f...c.bptc.ctex`
  - 配套 power 图标：`images/powers/diamond_diadem_power.png` → `.s3tc.ctex`（DXT5）
  - 图集引用：`images/atlases/relic_atlas.sprites/diamond_diadem.tres`、`relic_outline_atlas.sprites/...`

## 与 WebP 类 ctex 的关键差异

达弗 CG 那类图标/插画是**无损导入**（data_format=2，ctex 内嵌 WebP，数据从偏移 56 起）。而本图标被游戏导入成了**显卡压缩纹理**：

| | WebP 类 | BC7（bptc） | DXT5（s3tc） |
|---|---|---|---|
| data_format（头偏移 32 的 u32） | 2 | 22 | 19 |
| 数据起始偏移 | 56（52–55 为 WebP 长度） | **52** | **52** |
| 数据长度 | RIFF 头给出 | w×h×1 B（256²=65536） | 同左 |

ctex 头（前 20 字节）通用：`GST2` + u16 version=1 + u16 0 + u32 width + u32 height；本例 256×256、文件总长 65588 = 52 + 65536，无 mip。

## 解码方法

Python 依赖 **texture2ddecoder**（pip 直装可用，含 `decode_bc7` / `decode_bc3`）：

```python
rgba = texture2ddecoder.decode_bc7(data_65536B, 256, 256)  # -> RGBA bytes
Image.frombytes("RGBA", (256, 256), bytes(rgba)).save("out.png")
```

## 结果验证（透明背景达标）

- 四角及全部边缘抽样 alpha=0；全透明 34,900 px、半透明（抗锯齿）16,552 px、不透明 14,084 px；
- 中心为钻石头冠实体（灰白钻石 + 银色冠体），已目检确认。
