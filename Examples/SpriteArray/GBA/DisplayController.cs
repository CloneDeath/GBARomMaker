namespace GBA;

public static unsafe class DisplayController {
	private static readonly ushort* DISPCNT = (ushort*)0x04000000;
	private static readonly ushort* DISPSTAT = (ushort*)0x04000004;
	private static readonly ushort* IE = (ushort*)0x4000200;
	private static readonly uint* IME = (uint*)0x4000208;

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
		*destination = color.ToUShort();
	}

	public static void EnableVBlank() {
		*DISPSTAT |= 1 << 3;
    	*IE |= 1;
		*IME = 1;
	}
}
