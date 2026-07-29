using GBA;

unsafe
{
	//*(uint*)0x03007FFC = (uint)0x03000000;
	//*(ushort*)0x04000208 = 0x00000000;
	//*(ushort*)0x04000004 = 0x00000008;
	//*(ushort*)0x04000200 = 0x00000001;
	//*(ushort*)0x04000208 = 0x00000001;

	//*(uint*)0x04000208 = 0x00000001;
	//*(ushort*)0x04000200 = 0x0001; // maybe str instead of strh?
	DisplayController.EnableVBlank();

    *(ushort*)0x04000000 = 0x0403;
    *(ushort*)0x06000000 = 0x001F;	
	
	Interrupt.WaitVBlank();
    *(ushort*)0x06000002 = 0x03E0;	
	Interrupt.WaitVBlank();
    *(ushort*)0x06000004 = 0x7C00;	


	//vblank pls...
	//DisplayController.EnableVBlank();
	//DisplayController.SetControl(new DisplayControl {
	//	BGMode = 3,
	//	ScreenDisplayBG2 = true,
	//	ScreenDisplayOBJ = true
	//});
	////DisplayController.SetPixel(10, 20, Colors.Red);

	//DisplayController.SetPixel(0, 100, Colors.Red);
	//Interrupt.WaitVBlank();
	//DisplayController.SetPixel(1, 100, Colors.Green);
	//Interrupt.WaitVBlank();
	//DisplayController.SetPixel(2, 100, Colors.Blue);

	//ushort x = 0;
	//while(true){
	//	DisplayController.SetPixel(x++, 50, Colors.Green);
	//	Interrupt.WaitVBlank();
	//};

	// original
	
	//DisplayController.SetPixel(10, 20, Colors.Red);

    //// OBJ palette color 1 = red
    //((ushort*)0x05000200)[1] = 0x001F;	
	//byte* tile = (byte*)0x06014000;

	//// 8x8, 4bpp: two pixels per byte, both palette index 1
    //for (int i = 0; i < 32; i++)
    //    tile[i] = 0x11;

	//ushort* oam = (ushort*)0x07000000;

    //// Object 0: 8x8 at (0, 0)
    //oam[0] = 0;     // Y = 0
    //oam[1] = 8;     // X = 0
    //oam[2] = 512;   // Tile 512 => address 0x06014000

	//DisplayController.EnableVBlank();

	//ushort x = 0;
	//while(true){
	//	oam[1] = x++;
	//	if (x >= 200) {
	//		x = 0;
	//	}
	//	DisplayController.SetPixel(x, 50, Colors.Green);
	//	Interrupt.WaitVBlank();
	//};
}

