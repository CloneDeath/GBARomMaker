using System;
using GBA;

unsafe {
	DisplayController.EnableVBlank();
	DisplayController.SetControl(new DisplayControl {
		BGMode = 3,
		ScreenDisplayBG2 = true,
		ScreenDisplayOBJ = true
	});

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
	//	TileIndex = 513
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
		Interrupt.WaitVBlank();
	};



	//var spriteR = new Sprite(0) {
	//	TileIndex = 512
	//};
	//var spriteG = new Sprite(1) {
	//	TileIndex = 513
	//};
	//var spriteB = new Sprite(2) {
	//	TileIndex = 514
	//};

	//float timer = 0;
	//while(true){
	//	timer += 1f / 60;
	//	if (timer > 3) {
	//		timer = 0;
	//	}

	//	var circles = timer / 3;
	//	var radians = circles * 2 * MathF.PI;
	//	spriteR.Y = (byte)((System.MathF.Sin(radians) * 50) + 76);
	//	spriteR.X = (byte)((System.MathF.Cos(radians) * 50) + 116);
	//	spriteG.Y = (byte)((System.MathF.Sin(radians + (MathF.PI * 2f / 3)) * 50) + 76);
	//	spriteG.X = (byte)((System.MathF.Cos(radians + (MathF.PI * 2f / 3)) * 50) + 116);
	//	spriteB.Y = (byte)((System.MathF.Sin(radians + (MathF.PI * 4f / 3)) * 50) + 76);
	//	spriteB.X = (byte)((System.MathF.Cos(radians + (MathF.PI * 4f / 3)) * 50) + 116);
	//	Interrupt.WaitVBlank();
	//};
}

