using System;
using GBA;

unsafe {
	DisplayController.EnableVBlank();
	DisplayController.SetControl(new DisplayControl {
		BGMode = 3,
		ScreenDisplayBG2 = true,
		ScreenDisplayOBJ = true
	});

	//Sprite[] sprites = new Sprite[30];
	//for (byte i = 0; i < sprites.Length; i++) {
	//	byte palletIndex = (byte)(i + 1);
	//	Palette.SetObject(0, palletIndex, new Color { Red = i });

	//	byte tileIndex = (byte)(512 + i);
	//	for (byte y = 0; y < 8; y++) {
	//	for (byte x = 0; x < 8; x++) {
	//		CharacterData.SetColor(tileIndex, x, y, palletIndex);
	//	}
	//	}
	//	sprites[i] = new Sprite(i) {
	//		TileIndex = tileIndex,
	//		X = (byte)(i * 8),
	//		Y = 76
	//	};
	//}
	
	Palette.SetObject(0, 0, Colors.Red);
	Palette.SetObject(0, 1, Colors.Green);
	Palette.SetObject(0, 2, Colors.Blue);
	for (byte y = 0; y < 8; y++) {
	for (byte x = 0; x < 8; x++) {
		CharacterData.SetColor(513, x, y, 1);
	}
	}

	var sprites = new Sprite[1];

	sprites[0] = new Sprite(0) {
		TileIndex = 513,
		Y = 76
	};

	int timer = 0;
	while(true){
		timer += 1;
		sprites[0].Y = 76;
		sprites[0].X = (byte)(timer%232);
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
