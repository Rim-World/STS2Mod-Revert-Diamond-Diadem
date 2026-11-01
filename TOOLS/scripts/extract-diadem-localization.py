#!/usr/bin/env python3
"""从 v0.107 解包资源提取钻石头冠相关官方文本，生成本 mod 的各语言 localization json。

产出（写入 src/RevertDiamondDiadem/RevertDiamondDiadem/localization/<lang>/）：
  relics.json : RE_DIAMOND_DIADEM.description        （{CardThreshold} 占位符替换为字面 "2"）
  powers.json : REVERT_DIAMOND_DIADEM_POWER.title / .description / .smartDescription
                （键随自建 power 类的 slug 自动匹配，内容照抄 v0.107 的 DIAMOND_DIADEM_POWER.*）

缺失语言处理：
  zht  = 由 zhs 简体逐条转繁体（下方 ZHT_FROM_ZHS 对照表；源文变化时会报错提醒人工核对）；
  ind/tha = 107 无对应目录，以 eng 兜底（先例：回调飞拳 ADR-003）。
只读源文件，不修改任何既有内容。纯标准库。
"""
from __future__ import annotations

import json
import sys
from pathlib import Path

SRC_ROOT = Path(r"C:/Users/<用户名>/Desktop/Mod/STS2-Reference/1.STS2-107Reference/localization")
OUT_ROOT = Path(__file__).resolve().parent.parent / "src/RevertDiamondDiadem/RevertDiamondDiadem/localization"

LANGS = ["deu", "eng", "esp", "fra", "ind", "ita", "jpn", "kor",
         "pol", "ptb", "rus", "spa", "tha", "tur", "zhs", "zht"]

FALLBACK = "eng"

# 107 官方语言白名单（依据 1.STS2-107Reference/filelist.txt 解包清单实证：14 种，无 ind/zht）。
# 参考目录 localization/ 下混有从 111 复制来的 ind/zht 文件（MD5 与 111 解包一致），
# 白名单之外的语言一律不读参考目录，防止把 111 文案当成 107 文案用掉。
OFFICIAL_107_LANGS = {"deu", "eng", "esp", "fra", "ita", "jpn", "kor",
                      "pol", "ptb", "rus", "spa", "tha", "tur", "zhs"}

# zht 由 zhs 转繁体的对照表（键=v0.107 zhs 原文，值=对应繁体）。
# 若 zhs 源文与键不符（官方未来改动），立即报错退出，防止静默产出错误翻译。
ZHS_RELIK_DESC = "如果你在本回合打出的牌少于等于[blue]{CardThreshold}[/blue]张，则受到敌人的伤害减半。"
ZHT_FROM_ZHS = {
    ZHS_RELIK_DESC: "如果你在本回合打出的牌少於等於[blue]{CardThreshold}[/blue]張，則受到敵人的傷害減半。",
    "钻石头冠": "鑽石頭冠",
    "敌人在本回合造成的伤害减少[blue]50%[/blue]。": "敵人在本回合造成的傷害減少[blue]50%[/blue]。",
}


def load_table(lang: str, name: str) -> dict[str, str]:
    if lang not in OFFICIAL_107_LANGS:
        return {}
    path = SRC_ROOT / lang / name
    try:
        return json.loads(path.read_text(encoding="utf-8"))
    except FileNotFoundError:
        return {}


def s2t(text: str) -> str:
    """zhs→zht：查对照表；查不到说明 107 官方 zhs 源文已变化，直接报错。"""
    if text not in ZHT_FROM_ZHS:
        sys.exit(f"ERROR: zhs source text not in ZHT_FROM_ZHS table, manual review needed: {text!r}")
    return ZHT_FROM_ZHS[text]


def main() -> None:
    assert OUT_ROOT.is_absolute() and "RevertDiamondDiadem" in str(OUT_ROOT), OUT_ROOT

    eng_relics = load_table(FALLBACK, "relics.json")
    eng_powers = load_table(FALLBACK, "powers.json")

    for lang in LANGS:
        if lang == "zht":
            # 用户指定：繁体不取 eng 兜底，一律由 zhs 转换
            zhs_relics = load_table("zhs", "relics.json")
            zhs_powers = load_table("zhs", "powers.json")
            relic_desc = s2t(zhs_relics["DIAMOND_DIADEM.description"])
            p_title = s2t(zhs_powers["DIAMOND_DIADEM_POWER.title"])
            p_desc = s2t(zhs_powers["DIAMOND_DIADEM_POWER.description"])
            p_smart = s2t(zhs_powers["DIAMOND_DIADEM_POWER.smartDescription"])
            print(f"[{lang}] converted from zhs")
        else:
            relics = load_table(lang, "relics.json")
            powers = load_table(lang, "powers.json")

            # 逐项兜底：107 官方有该键就用官方文案，缺哪个键才用 eng 补哪个。
            # 典型如 tha：官方泰语缺 DIAMOND_DIADEM_POWER.title（游戏内本就回落 eng 表），
            # 但 description/smartDescription 是完整的 107 官方泰语，应原样保留。
            relic_desc = relics.get("DIAMOND_DIADEM.description") or eng_relics["DIAMOND_DIADEM.description"]
            p_title = powers.get("DIAMOND_DIADEM_POWER.title") or eng_powers["DIAMOND_DIADEM_POWER.title"]
            p_desc = powers.get("DIAMOND_DIADEM_POWER.description") or eng_powers["DIAMOND_DIADEM_POWER.description"]
            p_smart = powers.get("DIAMOND_DIADEM_POWER.smartDescription") or eng_powers["DIAMOND_DIADEM_POWER.smartDescription"]
            if lang not in OFFICIAL_107_LANGS:
                print(f"[{lang}] not in v0.107 official languages, using eng fallback for all keys")
            elif relic_desc != relics.get("DIAMOND_DIADEM.description") or p_title != powers.get("DIAMOND_DIADEM_POWER.title"):
                print(f"[{lang}] partial eng fallback for missing official keys")

        lang_dir = OUT_ROOT / lang
        lang_dir.mkdir(parents=True, exist_ok=True)

        (lang_dir / "relics.json").write_text(
            json.dumps({"RE_DIAMOND_DIADEM.description": relic_desc.replace("{CardThreshold}", "2")},
                       ensure_ascii=False, indent=2) + "\n",
            encoding="utf-8")

        (lang_dir / "powers.json").write_text(
            json.dumps({
                "REVERT_DIAMOND_DIADEM_POWER.title": p_title,
                "REVERT_DIAMOND_DIADEM_POWER.description": p_desc,
                "REVERT_DIAMOND_DIADEM_POWER.smartDescription": p_smart,
            }, ensure_ascii=False, indent=2) + "\n",
            encoding="utf-8")

        print(f"[{lang}] written")


if __name__ == "__main__":
    main()
