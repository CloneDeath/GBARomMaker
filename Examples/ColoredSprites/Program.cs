using System;
using GBA;

unsafe {
	DisplayController.SetControl(new DisplayControl {
		BGMode = 3,
		ScreenDisplayBG2 = true,
		ScreenDisplayOBJ = true
	});
	DisplayController.SetPixel(10, 20, Colors.Red);

	Palette.SetObject(0, 1, Colors.Red);
    //((ushort*)0x05000200)[1] = Colors.Red.ToUShort();	
	byte* tile = (byte*)0x06014000;

	// 8x8, 4bpp: two pixels per byte, both palette index 1
    for (int i = 0; i < 32; i++)
        tile[i] = 0x11;

	var sprite = new Sprite(0);
	sprite.X = 8;
	sprite.Y = 0;
    sprite.TileIndex = 512; // Tile 512 => address 0x06014000

	DisplayController.EnableVBlank();
	float timer = 0;
	while(true){
		timer += 1f / 60;
		if (timer > 3) {
			timer = 0;
		}

		var circles = timer / 3;
		var radians = circles * 2 * MathF.PI;
		sprite.Y = (byte)((System.MathF.Sin(radians) * 50) + 80);
		sprite.X = (byte)((System.MathF.Cos(radians) * 50) + 80);
		Interrupt.WaitVBlank();
	};
}

