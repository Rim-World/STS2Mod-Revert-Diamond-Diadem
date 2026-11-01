#!/usr/bin/env python3
"""Godot pck 目录搜索与定点提取工具（纯标准库）。

用法：
  python pck-tool.py search  <pck> <子串>              # 列出路径含子串的条目
  python pck-tool.py extract <pck> <子串> <输出目录>   # 把匹配条目按原相对路径写出
"""
from __future__ import annotations

import struct
import sys
from pathlib import Path


def parse(path: str):
    with open(path, "rb") as f:
        if f.read(4) != b"GDPC":
            raise ValueError("not a GDPC pck")
        ver = struct.unpack("<I", f.read(4))[0]
        if ver <= 2:
            raise ValueError(f"unsupported pack version {ver}")
        f.read(12)   # engine major/minor/patch
        f.read(4)    # pack flags
        file_base = struct.unpack("<Q", f.read(8))[0]
        dir_off = struct.unpack("<Q", f.read(8))[0]
        f.seek(dir_off)
        count = struct.unpack("<I", f.read(4))[0]
        entries = []
        for _ in range(count):
            ln = struct.unpack("<I", f.read(4))[0]
            name = f.read(ln).split(b"\x00", 1)[0].decode("utf-8", "replace")
            off = struct.unpack("<Q", f.read(8))[0]
            size = struct.unpack("<Q", f.read(8))[0]
            f.read(16)  # md5
            f.read(4)   # flags
            entries.append((name, file_base + off, size))
    return entries


def main() -> int:
    if len(sys.argv) < 4:
        print(__doc__)
        return 1
    cmd, pck, needle = sys.argv[1], sys.argv[2], sys.argv[3]
    entries = parse(pck)
    hits = [(n, o, s) for n, o, s in entries if needle.lower() in n.lower()]
    print(f"total {len(entries)} entries, {len(hits)} matched '{needle}'")
    if cmd == "search":
        for n, o, s in hits:
            print(f"  {n}  ({s} bytes)")
        return 0
    if cmd == "extract":
        out_root = Path(sys.argv[4])
        for n, o, s in hits:
            dest = out_root / n.lstrip("/\\").replace("res://", "")
            dest.parent.mkdir(parents=True, exist_ok=True)
            with open(pck, "rb") as f:
                f.seek(o)
                dest.write_bytes(f.read(s))
            print(f"  -> {dest} ({s} bytes)")
        return 0
    print(__doc__)
    return 1


if __name__ == "__main__":
    sys.exit(main())
