namespace GBA;

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

	public static void SetPixel(int x, int y, Color color) {
    	ushort* topLeftPixel = (ushort*)0x06000000;
		ushort* destination = topLeftPixel+x+(y*240);
		ushort value = 0x0000;
    	value = (ushort)(color.Red & 0x001F);
    	value |= (ushort)((color.Green & 0x001F) << 5);
    	value |= (ushort)((color.Blue & 0x001F) << 10);
		*destination = value;
	}
}
