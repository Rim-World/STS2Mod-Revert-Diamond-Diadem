#!/usr/bin/env python3
"""生成占位 mod_image.png（420x420 纯色底 + 中央简单皇冠形状），纯标准库 zlib+struct。

正式宣传图就位后可直接覆盖 src/RevertDiamondDiadem/RevertDiamondDiadem/mod_image.png。
"""
from __future__ import annotations

import struct
import zlib
from pathlib import Path

OUT = Path(__file__).resolve().parent.parent / "src/RevertDiamondDiadem/RevertDiamondDiadem/mod_image.png"
SIZE = 420
BG = (0x22, 0x22, 0x33)
FG = (0xE8, 0xC3, 0x4A)


def crown_mask(x: int, y: int) -> bool:
    """皇冠粗略像素形状：三尖一底带，中央偏上放一颗小圆点当宝石。"""
    cx = SIZE // 2
    dy = y - 210          # 相对中心
    dx = x - cx
    base_band = -90 <= dy <= -55 and abs(dx) <= 110
    left_spike = abs(dx + 75) <= 14 and -165 <= dy < -60
    mid_spike = abs(dx) <= 16 and -195 <= dy < -70
    right_spike = abs(dx - 75) <= 14 and -165 <= dy < -60
    gem = (dx * dx + (dy + 72) ** 2) ** 0.5 <= 11
    return base_band or left_spike or mid_spike or right_spike or gem


def build_png() -> bytes:
    raw = bytearray()
    for y in range(SIZE):
        raw.append(0)  # PNG filter type 0
        for x in range(SIZE):
            color = FG if crown_mask(x, y) else BG
            raw.extend(color)
            raw.append(0xFF)

    def chunk(tag: bytes, data: bytes) -> bytes:
        body = tag + data
        return struct.pack(">I", len(data)) + body + struct.pack(">I", zlib.crc32(body) & 0xFFFFFFFF)

    ihdr = struct.pack(">IIBBBBB", SIZE, SIZE, 8, 6, 0, 0, 0)  # 8bit RGBA
    return (b"\x89PNG\r\n\x1a\n"
            + chunk(b"IHDR", ihdr)
            + chunk(b"IDAT", zlib.compress(bytes(raw), 9))
            + chunk(b"IEND", b""))


def main() -> None:
    OUT.parent.mkdir(parents=True, exist_ok=True)
    OUT.write_bytes(build_png())
    print(f"written {OUT} ({OUT.stat().st_size} bytes)")


if __name__ == "__main__":
    main()
