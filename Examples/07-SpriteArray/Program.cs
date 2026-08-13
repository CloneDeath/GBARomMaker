using System;
using GBA;

unsafe {
	DisplayController.EnableVBlank();
	DisplayController.SetControl(new DisplayControl {
		BGMode = 3,
		ScreenDisplayBG2 = true,
		ScreenDisplayOBJ = true
	});

	Sprite[] sprites = new Sprite[30];
	for (byte i = 0; i < sprites.Length; i++) {
		byte colorIndex = (byte)((i % 15) + 1);
		byte paletteIndex = (byte)(i / 15);
		Palette.SetObject(paletteIndex, colorIndex, ColorFromHSV(i * (360f / sprites.Length), 1, 1));

		ushort tileIndex = (ushort)(512 + i);
		for (byte y = 0; y < 8; y++) {
		for (byte x = 0; x < 8; x++) {
			CharacterData.SetColor(tileIndex, x, y, colorIndex);
		}
		}
		sprites[i] = new Sprite(i) {
			TileIndex = tileIndex,
			X = (byte)(i * 8),
			Y = 76,
			PaletteIndex = paletteIndex
		};
	}
	
	var circles = 0f;
	while(true) {
		circles += 1/60f;
		if (circles >= 2) {
			circles -= 1;
		}
		for (var i = 0; i < sprites.Length; i++) {
			var offset = ((float)i)/sprites.Length;
			if (circles < offset) {
				sprites[i].Y = 76;
			} else {
				sprites[i].Y = (byte)(50 * MathF.Sin((circles - offset) * MathF.PI));
			}
		}
		Interrupt.WaitVBlank();
	};
}

static Color ColorFromHSV(float hue, float saturation, float value) {
    int hi = Convert.ToInt32(MathF.Floor(hue / 60)) % 6;
    float f = hue / 60 - MathF.Floor(hue / 60);

    value = value * 31;
    byte v = Convert.ToByte(value);
    byte p = Convert.ToByte(value * (1 - saturation));
    byte q = Convert.ToByte(value * (1 - f * saturation));
    byte t = Convert.ToByte(value * (1 - (1 - f) * saturation));

    if (hi == 0)
        return new Color { Red = v, Green = t, Blue = p };
    else if (hi == 1)
        return new Color { Red = q, Green = v, Blue = p };
    else if (hi == 2)
        return new Color { Red = p, Green = v, Blue = t };
    else if (hi == 3)
        return new Color { Red = p, Green = q, Blue = v };
    else if (hi == 4)
        return new Color { Red = t, Green = p, Blue = v };
    else
        return new Color { Red = v, Green = p, Blue = q };
}
