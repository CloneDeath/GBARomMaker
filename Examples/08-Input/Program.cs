using GBA;

unsafe {
	DisplayController.EnableVBlank();
	DisplayController.SetControl(new DisplayControl {
		BGMode = 3,
		ScreenDisplayBG2 = true,
		ScreenDisplayOBJ = true
	});

	Palette.SetObject(0, 1, Colors.Red);
	Palette.SetObject(1, 1, Colors.Green);
	for (byte y = 0; y < 8; y++) {
	for (byte x = 0; x < 8; x++) {
		CharacterData.SetColor(512, x, y, 1);
	}
	}

	var up = new Sprite(0) {
		TileIndex = 512,
		X = 0,
		Y = 76,
		PaletteIndex = 0
	};

	while(true) {
		up.PaletteIndex = (byte)(Input.Up ? 1 : 0);
		Interrupt.WaitVBlank();
	};
}

