import math
import re
import secrets
from array import array
from pathlib import Path

from fontTools.ttLib import TTFont
from fontTools.ttLib.tables._g_l_y_f import Glyph, GlyphComponent, GlyphCoordinates
from fontTools.ttLib.tables.ttProgram import Program
from fontTools.pens.ttGlyphPen import TTGlyphPen

HERE = Path(__file__).parent
SRC = HERE / "tannenbergfett.original.ttf"
SCSS_ROOT = HERE.parent.parent / "code" / "UI"

TAG = secrets.token_hex(3)
FAMILY = f"Hamster-{TAG}"
FILENAME = f"hamster-{TAG}.ttf"

RING_CX, RING_CY = 261, 703
RING_OUTER = 110
RING_INNER = 64

ARROW_CX = 300
ARROW_CY = 350
ARROW_HALF = 200
ARROW_STEM_HALF = 60
ARROW_HEAD_BACK = -20
ARROW_ADV = ARROW_CX * 2

ACCENT_ADV = 522
CAP_SHIFT = 140

ARROWS = {
	0xE000: ("arrowup",    "up"),
	0xE001: ("arrowright", "right"),
	0xE002: ("arrowdown",  "down"),
	0xE003: ("arrowleft",  "left"),
}

# (codepoint, glyph name, base glyph, accent glyph, y shift for uppercase)
COMPOSITES = [
	(0x00E0, "agrave",      "a",        "grave",      0),
	(0x00E1, "aacute",      "a",        "acute",      0),
	(0x00E7, "ccedilla",    "c",        "cedilla",    0),
	(0x00E9, "eacute",      "e",        "acute",      0),
	(0x00ED, "iacute",      "dotlessi", "acute",      0),
	(0x00F1, "ntilde",      "n",        "tilde",      0),
	(0x00F3, "oacute",      "o",        "acute",      0),
	(0x00F4, "ocircumflex", "o",        "circumflex", 0),
	(0x00F9, "ugrave",      "u",        "grave",      0),
	(0x00FA, "uacute",      "u",        "acute",      0),
	(0x00FD, "yacute",      "y",        "acute",      0),
	(0x010C, "Ccaron",      "C",        "caron",      CAP_SHIFT),
	(0x010D, "ccaron",      "c",        "caron",      0),
	(0x010E, "Dcaron",      "D",        "caron",      CAP_SHIFT),
	(0x010F, "dcaron",      "d",        "caron",      0),
	(0x011A, "Ecaron",      "E",        "caron",      CAP_SHIFT),
	(0x011B, "ecaron",      "e",        "caron",      0),
	(0x0147, "Ncaron",      "N",        "caron",      CAP_SHIFT),
	(0x0148, "ncaron",      "n",        "caron",      0),
	(0x0158, "Rcaron",      "R",        "caron",      CAP_SHIFT),
	(0x0159, "rcaron",      "r",        "caron",      0),
	(0x0160, "Scaron",      "S",        "caron",      CAP_SHIFT),
	(0x0161, "scaron",      "s",        "caron",      0),
	(0x0164, "Tcaron",      "T",        "caron",      CAP_SHIFT),
	(0x0165, "tcaron",      "t",        "caron",      0),
	(0x016E, "Uring",       "U",        "ringabove",  CAP_SHIFT),
	(0x016F, "uring",       "u",        "ringabove",  0),
	(0x017D, "Zcaron",      "Z",        "caron",      CAP_SHIFT),
	(0x017E, "zcaron",      "z",        "caron",      0),
	(0x0457, "iidieresis",  "dotlessi", "dieresis",   0),   # ї
	(0x045E, "ubreve_cyr",  "uni0443",  "breve",      0),   # ў
]

# cyrillic codepoints that can reuse an existing glyph's shape
ALIASES = [
	(0x0406, "I"),          # І
	(0x0456, "i"),          # і
	(0x0454, "uni044D"),    # є -> cyrillic э
	(0x0491, "uni0433"),    # ґ -> cyrillic г
]

FAMILY_RE = re.compile(r"font-family:\s*(?:Hamster-[a-f0-9]+|Tannenberg Fett)\b", re.IGNORECASE)


def register(font, name, glyph, advance):
	font["glyf"][name] = glyph
	font["hmtx"][name] = (advance, 0)
	order = font.getGlyphOrder()
	if name not in order:
		order.append(name)
		font.setGlyphOrder(order)


def visual_center(font, name):
	glyf = font["glyf"]
	g = glyf[name]
	if g.numberOfContours < 0:
		g.recalcBounds(glyf)
	if not hasattr(g, "xMin"):
		adv, _ = font["hmtx"][name]
		return adv // 2
	return (g.xMin + g.xMax) // 2


def set_cmap(font, cp, glyph_name):
	for sub in font["cmap"].tables:
		if sub.isUnicode():
			sub.cmap[cp] = glyph_name


def circle_ring(pen, cx, cy, r_outer, r_inner, segments=28):
	for i in range(segments):
		a = i * 2 * math.pi / segments
		x = cx + r_outer * math.cos(a)
		y = cy + r_outer * math.sin(a)
		(pen.moveTo if i == 0 else pen.lineTo)((round(x), round(y)))
	pen.closePath()
	# inner contour, opposite winding carves the hole
	for i in range(segments):
		a = -i * 2 * math.pi / segments
		x = cx + r_inner * math.cos(a)
		y = cy + r_inner * math.sin(a)
		(pen.moveTo if i == 0 else pen.lineTo)((round(x), round(y)))
	pen.closePath()


def ring_glyph():
	pen = TTGlyphPen(None)
	circle_ring(pen, RING_CX, RING_CY, RING_OUTER, RING_INNER)
	return pen.glyph()


def arrow_polygon(direction, cx, cy, half, stem_half, head_back):
	# baseline points right, head spans full ±half square so all 4 rotations share bounds
	back_x = cx - head_back
	pts = [
		(cx - half, cy + stem_half),
		(back_x,    cy + stem_half),
		(back_x,    cy + half),
		(cx + half, cy),
		(back_x,    cy - half),
		(back_x,    cy - stem_half),
		(cx - half, cy - stem_half),
	]

	def rot(p, deg):
		px, py = p[0] - cx, p[1] - cy
		r = math.radians(deg)
		c, s = math.cos(r), math.sin(r)
		return (round(cx + px * c - py * s), round(cy + px * s + py * c))

	angles = {"right": 0, "up": 90, "left": 180, "down": -90}
	return [rot(p, angles[direction]) for p in pts]


def arrow_glyph(direction):
	pen = TTGlyphPen(None)
	pts = arrow_polygon(direction, ARROW_CX, ARROW_CY, ARROW_HALF, ARROW_STEM_HALF, ARROW_HEAD_BACK)
	pen.moveTo(pts[0])
	for p in pts[1:]:
		pen.lineTo(p)
	pen.closePath()
	return pen.glyph()


def make_dotlessi(font):
	# keep only contour 0 of i, drops the dot on top so diacritic vowels like í don't double up
	src = font["glyf"]["i"]
	end0 = src.endPtsOfContours[0]
	n = end0 + 1
	g = Glyph()
	g.numberOfContours = 1
	g.endPtsOfContours = [end0]
	coords = [src.coordinates[i] for i in range(n)]
	g.coordinates = GlyphCoordinates(coords)
	g.flags = array("B", [src.flags[i] for i in range(n)])
	g.program = Program()
	g.program.fromBytecode(b"")
	g.xMin = min(p[0] for p in coords)
	g.xMax = max(p[0] for p in coords)
	g.yMin = min(p[1] for p in coords)
	g.yMax = max(p[1] for p in coords)
	register(font, "dotlessi", g, font["hmtx"]["i"][0])


def make_flipped_accent(font, name, source):
	# vertical flip of circumflex gives us a caron / breve that matches the designer's blackletter weight
	cf = font["glyf"][source]
	comp = GlyphComponent()
	comp.glyphName = source
	comp.x = 0
	comp.y = cf.yMin + cf.yMax
	comp.transform = [[1.0, 0.0], [0.0, -1.0]]
	comp.flags = 0x200
	g = Glyph()
	g.numberOfContours = -1
	g.components = [comp]
	register(font, name, g, font["hmtx"][source][0])


def make_composite(font, new_name, base, accent, y_shift):
	base_center = visual_center(font, base)
	accent_center = visual_center(font, accent)
	dx = base_center - accent_center

	cb = GlyphComponent()
	cb.glyphName = base
	cb.x = 0
	cb.y = 0
	cb.flags = 0x204  # ARGS_ARE_XY | MORE_COMPONENTS

	ca = GlyphComponent()
	ca.glyphName = accent
	ca.x = dx
	ca.y = y_shift
	ca.flags = 0x200

	g = Glyph()
	g.numberOfContours = -1
	g.components = [cb, ca]
	register(font, new_name, g, font["hmtx"][base][0])


def clean_previous():
	for old in list(HERE.glob("hamster-*.ttf")) + list(HERE.glob("hamster-*.ttf_c")):
		try:
			old.unlink()
		except OSError:
			pass


def rewrite_scss():
	count = 0
	for path in SCSS_ROOT.rglob("*.scss"):
		text = path.read_text(encoding="utf-8")
		new = FAMILY_RE.sub(f"font-family: {FAMILY}", text)
		if new != text:
			path.write_text(new, encoding="utf-8")
			count += 1
	print(f"patched {count} scss files")


def main():
	clean_previous()

	font = TTFont(str(SRC))

	# rebuild accents first, composites below reference them
	make_dotlessi(font)
	make_flipped_accent(font, "caron", "circumflex")
	make_flipped_accent(font, "breve", "circumflex")
	register(font, "ringabove", ring_glyph(), ACCENT_ADV)
	# pin ringabove LSB to ring xMin, register() sets it to 0 which shoves ů
	font["hmtx"]["ringabove"] = (ACCENT_ADV, RING_CX - RING_OUTER)

	# arrows in pua
	for cp, (name, direction) in ARROWS.items():
		register(font, name, arrow_glyph(direction), ARROW_ADV)
		set_cmap(font, cp, name)

	# precomposed accented letters aligned by visual center
	for cp, name, base, accent, y in COMPOSITES:
		make_composite(font, name, base, accent, y)
		set_cmap(font, cp, name)

	# cyrillic letters that can share an existing shape
	for cp, existing in ALIASES:
		set_cmap(font, cp, existing)

	# pin uring hmtx so composite bounds don't shove ů sideways
	font["hmtx"]["uring"] = font["hmtx"].metrics.get("u", (513, -11))

	# bump family every run so s&box can't serve a cached atlas
	for record in list(font["name"].names):
		if record.nameID in (1, 4, 6, 16):
			try:
				record.string = FAMILY.encode(record.getEncoding(), errors="replace")
			except Exception:
				record.string = FAMILY

	dst = HERE / FILENAME
	font.save(str(dst))
	print(f"built {dst.name}  family={FAMILY}")
	rewrite_scss()


if __name__ == "__main__":
	main()
