import argparse
import io
import json
import re
import sys
from pathlib import Path

from fontTools.ttLib import TTFont

sys.stdout = io.TextIOWrapper(sys.stdout.buffer, encoding="utf-8")

HERE = Path(__file__).parent
PROJECT = HERE.parent.parent
LOCALIZATION = PROJECT / "Assets" / "localization"
L_CS = PROJECT / "code" / "Localization" / "L.cs"

PROBES = [
	# western european
	("à", 0x00E0),
	("á", 0x00E1),
	("ç", 0x00E7),
	("é", 0x00E9),
	("í", 0x00ED),
	("ñ", 0x00F1),
	("ó", 0x00F3),
	("ô", 0x00F4),
	("ù", 0x00F9),
	("ú", 0x00FA),
	("ý", 0x00FD),
	# czech
	("Č", 0x010C),
	("č", 0x010D),
	("Ď", 0x010E),
	("ď", 0x010F),
	("Ě", 0x011A),
	("ě", 0x011B),
	("Ň", 0x0147),
	("ň", 0x0148),
	("Ř", 0x0158),
	("ř", 0x0159),
	("Š", 0x0160),
	("š", 0x0161),
	("Ť", 0x0164),
	("ť", 0x0165),
	("Ů", 0x016E),
	("ů", 0x016F),
	("Ž", 0x017D),
	("ž", 0x017E),
	# cyrillic aliases and composites
	("І", 0x0406),
	("і", 0x0456),
	("є", 0x0454),
	("ї", 0x0457),
	("ў", 0x045E),
	("ґ", 0x0491),
	# pua arrows
	("arrow up", 0xE000),
	("arrow right", 0xE001),
	("arrow down", 0xE002),
	("arrow left", 0xE003),
]

SUMMARY_GLYPHS = (
	"a", "c", "e", "i", "o", "r", "s", "u", "y", "z",
	"acute", "caron", "circumflex", "ringabove",
	"aacute", "ccaron", "eacute", "uring",
	"arrowup", "arrowright", "arrowdown", "arrowleft",
)


def pick_font(arg):
	if arg:
		return Path(arg)
	candidates = sorted(HERE.glob("hamster-*.ttf"))
	if candidates:
		return candidates[-1]
	return HERE / "tannenbergfett.original.ttf"


def summary(font, path):
	print(f"file: {path.name}")
	family = next((n.toUnicode() for n in font["name"].names if n.nameID == 1), "?")
	print(f"family: {family}")
	print(f"upm: {font['head'].unitsPerEm}")

	cmap = font.getBestCmap()
	print(f"total chars: {len(cmap)}")

	for label, cp in PROBES:
		name = cmap.get(cp)
		mark = "ok " if name else "-- "
		print(f"  {mark}{label} (U+{cp:04X}) -> {name}")

	hmtx = font["hmtx"]
	glyf = font["glyf"]
	for name in SUMMARY_GLYPHS:
		if name not in hmtx.metrics:
			continue
		adv, lsb = hmtx.metrics[name]
		g = glyf[name]
		if g.numberOfContours == -1:
			kind = "composite " + str([(c.glyphName, c.x, c.y) for c in g.components])
		else:
			kind = f"bounds {g.xMin},{g.yMin} -> {g.xMax},{g.yMax}"
		print(f"  {name:12}  adv={adv:4}  lsb={lsb:4}  {kind}")


def glyph_list(font):
	order = font.getGlyphOrder()
	print(f"{len(order)} glyphs:")
	for i, name in enumerate(order):
		print(f"  {i:4}  {name}")


def composites(font):
	glyf = font["glyf"]
	hmtx = font["hmtx"]
	any_found = False
	for name in font.getGlyphOrder():
		g = glyf[name]
		if g.numberOfContours != -1:
			continue
		any_found = True
		adv, _ = hmtx.metrics[name]
		parts = [f"{c.glyphName}@({c.x},{c.y})" for c in g.components]
		print(f"  {name:16}  adv={adv:4}  {parts}")
	if not any_found:
		print("  (no composites)")


def cmap_tables(font):
	for sub in font["cmap"].tables:
		if not sub.isUnicode():
			continue
		entries = len(sub.cmap)
		print(f"fmt={sub.format} platform={sub.platformID}/{sub.platEncID} lang={sub.language} entries={entries}")
		# probe each diacritic and arrow per subtable so subtable drift is visible
		for label, cp in PROBES:
			print(f"  U+{cp:04X} {label:8} -> {sub.cmap.get(cp)}")


def coverage(font):
	cmap = font.getBestCmap()
	chars = set()

	for jf in sorted(LOCALIZATION.glob("*.json")):
		try:
			data = json.loads(jf.read_text(encoding="utf-8"))
		except Exception as e:
			print(f"  skip {jf.name}: {e}")
			continue
		for value in data.values():
			if isinstance(value, str):
				chars.update(value)

	# L.Available titles aren't in json, pull them out of L.cs
	if L_CS.exists():
		for match in re.finditer(r'new Entry\(\s*"[^"]+"\s*,\s*"([^"]+)"\s*\)', L_CS.read_text(encoding="utf-8")):
			chars.update(match.group(1))

	missing = sorted(c for c in chars if c != " " and ord(c) not in cmap)
	if not missing:
		print(f"all {len(chars)} chars renderable")
		return
	print(f"{len(missing)} chars missing:")
	for c in missing:
		print(f"  U+{ord(c):04X}  {c!r}")


def main():
	parser = argparse.ArgumentParser(description="inspect the built or source font")
	parser.add_argument("--font", help="path to a .ttf, defaults to newest hamster-*.ttf")
	parser.add_argument("--glyphs", action="store_true", help="dump full glyph order")
	parser.add_argument("--composites", action="store_true", help="list every composite glyph and its parts")
	parser.add_argument("--coverage", action="store_true", help="scan locale json and L.Available for chars not in the font")
	parser.add_argument("--cmap", action="store_true", help="dump every unicode cmap subtable and its probe hits")
	args = parser.parse_args()

	path = pick_font(args.font)
	font = TTFont(str(path))

	if args.glyphs:
		glyph_list(font)
	elif args.composites:
		composites(font)
	elif args.coverage:
		coverage(font)
	elif args.cmap:
		cmap_tables(font)
	else:
		summary(font, path)


if __name__ == "__main__":
	main()
