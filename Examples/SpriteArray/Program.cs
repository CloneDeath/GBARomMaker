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
		sprites[i] = new Sprite(i);
	}
	
	Palette.SetObject(0, 1, Colors.Red);
	Palette.SetObject(0, 2, Colors.Green);
	Palette.SetObject(0, 3, Colors.Blue);
	for (byte y = 0; y < 8; y++) {
	for (byte x = 0; x < 8; x++) {
		CharacterData.SetColor(512, x, y, 1);
		CharacterData.SetColor(513, x, y, 2);
		CharacterData.SetColor(514, x, y, 3);
	}
	}

	var spriteR = new Sprite(0) {
		TileIndex = 512
	};
	var spriteG = new Sprite(1) {
		TileIndex = 513
	};
	var spriteB = new Sprite(2) {
		TileIndex = 514
	};

	float timer = 0;
	while(true){
		timer += 1f / 60;
		if (timer > 3) {
			timer = 0;
		}

		var circles = timer / 3;
		var radians = circles * 2 * MathF.PI;
		spriteR.Y = (byte)((System.MathF.Sin(radians) * 50) + 76);
		spriteR.X = (byte)((System.MathF.Cos(radians) * 50) + 116);
		spriteG.Y = (byte)((System.MathF.Sin(radians + (MathF.PI * 2f / 3)) * 50) + 76);
		spriteG.X = (byte)((System.MathF.Cos(radians + (MathF.PI * 2f / 3)) * 50) + 116);
		spriteB.Y = (byte)((System.MathF.Sin(radians + (MathF.PI * 4f / 3)) * 50) + 76);
		spriteB.X = (byte)((System.MathF.Cos(radians + (MathF.PI * 4f / 3)) * 50) + 116);
		Interrupt.WaitVBlank();
	};
}

static Color ColorFromHSV(float hue, float saturation, float value)
{
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
