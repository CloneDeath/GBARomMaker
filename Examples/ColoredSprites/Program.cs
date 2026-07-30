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
	byte x = 0;
	//float y = 0;
	while(true){
		sprite.X = x++;
		if (x >= 232) {
			x = 0;
			//y = 0;
		}
		//y += 1.5f;
		//oam[0] = (ushort)y;
		DisplayController.SetPixel(x, 50, Colors.Green);
		Interrupt.WaitVBlank();
	};
}

