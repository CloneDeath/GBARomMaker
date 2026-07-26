unsafe
{
	DisplayController.SetControl(new DisplayControl {
		BGMode = 3,
		ScreenDisplayBG2 = true,
		ScreenDisplayOBJ = true
	});
	
	DisplayController.SetPixelRed(10, 20);

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

	while(true){};
}

public class DisplayControl {
	public int BGMode;
	public bool ScreenDisplayBG0;
	public bool ScreenDisplayBG1;
	public bool ScreenDisplayBG2;
	public bool ScreenDisplayBG3;
	public bool ScreenDisplayOBJ;
}

public static unsafe class DisplayController {
	private static ushort* DISPCNT = (ushort*)0x04000000;

	public static void SetControl(DisplayControl control) {
		ushort data = 0x0000;
		data |= (ushort)(control.BGMode & 0b111);
		data |= (ushort)((control.ScreenDisplayBG0 ? 1 : 0) << 8);
		data |= (ushort)((control.ScreenDisplayBG1 ? 1 : 0) << 9);
		data |= (ushort)((control.ScreenDisplayBG2 ? 1 : 0) << 10);
		data |= (ushort)((control.ScreenDisplayBG3 ? 1 : 0) << 11);
		data |= (ushort)((control.ScreenDisplayOBJ ? 1 : 0) << 12);
		*DISPCNT = data;
	}

	public static void SetPixelRed(int x, int y) {
    	ushort* topLeftPixel = (ushort*)0x06000000;
    	*(topLeftPixel+x+(y*240)) = 0x001F; // Red
	}
}
