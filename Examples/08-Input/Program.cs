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
		X = 32,
		Y = 68,
		PaletteIndex = 0
	};
	var down = new Sprite(1) {
		TileIndex = 512,
		X = 32,
		Y = 84,
		PaletteIndex = 0
	};
	var left = new Sprite(2) {
		TileIndex = 512,
		X = 24,
		Y = 76,
		PaletteIndex = 0
	};
	var right = new Sprite(3) {
		TileIndex = 512,
		X = 40,
		Y = 76,
		PaletteIndex = 0
	};
	
	var a = new Sprite(4) {
		TileIndex = 512,
		X = 140,
		Y = 68,
		PaletteIndex = 0
	};
	var b = new Sprite(5) {
		TileIndex = 512,
		X = 132,
		Y = 76,
		PaletteIndex = 0
	};
	var start = new Sprite(6) {
		TileIndex = 512,
		X = 48,
		Y = 96,
		PaletteIndex = 0
	};
	var select = new Sprite(7) {
		TileIndex = 512,
		X = 48,
		Y = 112,
		PaletteIndex = 0
	};
	var l = new Sprite(8) {
		TileIndex = 512,
		X = 40,
		Y = 52,
		PaletteIndex = 0
	};
	var r = new Sprite(9) {
		TileIndex = 512,
		X = 140,
		Y = 52,
		PaletteIndex = 0
	};

	while(true) {
		up.PaletteIndex = (byte)(Input.Up ? 1 : 0);
		down.PaletteIndex = (byte)(Input.Down ? 1 : 0);
		left.PaletteIndex = (byte)(Input.Left ? 1 : 0);
		right.PaletteIndex = (byte)(Input.Right ? 1 : 0);
		a.PaletteIndex = (byte)(Input.A ? 1 : 0);
		b.PaletteIndex = (byte)(Input.B ? 1 : 0);
		start.PaletteIndex = (byte)(Input.Start ? 1 : 0);
		select.PaletteIndex = (byte)(Input.Select ? 1 : 0);
		l.PaletteIndex = (byte)(Input.L ? 1 : 0);
		r.PaletteIndex = (byte)(Input.R ? 1 : 0);
		Interrupt.WaitVBlank();
	};
}

