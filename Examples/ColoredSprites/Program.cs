using GBA;

unsafe {
	DisplayController.SetControl(new DisplayControl {
		BGMode = 3,
		ScreenDisplayBG2 = true,
		ScreenDisplayOBJ = true
	});
	DisplayController.SetPixel(10, 20, Colors.Red);

    // OBJ palette color 1 = red
    ((ushort*)0x05000200)[1] = 0x001F;	
	byte* tile = (byte*)0x06014000;

	// 8x8, 4bpp: two pixels per byte, both palette index 1
    for (int i = 0; i < 32; i++)
        tile[i] = 0x11;

	ushort* oam = (ushort*)0x07000000;

    // Object 0: 8x8 at (0, 0)
    oam[0] = 0;     // Y = 0
    oam[1] = 8;     // X = 0
    oam[2] = 512;   // Tile 512 => address 0x06014000

	DisplayController.EnableVBlank();
	ushort x = 0;
	while(true){
		oam[1] = x++;
		if (x >= 232) {
			x = 0;
		}
		DisplayController.SetPixel(x, 50, Colors.Green);
		Interrupt.WaitVBlank();
	};
}

