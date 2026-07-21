unsafe
{
	DisplayController.SetControl(new DisplayControl {
		BGMode = 3,
		ScreenDisplayBG2 = true
	});

	for (var i = 0; i <= 10; i++) {
		DisplayController.SetPixelRed(i, 0);
		DisplayController.SetPixelRed(i, 10);
		DisplayController.SetPixelRed(0, i);
		DisplayController.SetPixelRed(10, i);
	}
}

public class DisplayControl {
	public int BGMode;
	public bool ScreenDisplayBG2;
}

public static unsafe class DisplayController {
	private static ushort* DISPCNT = (ushort*)0x04000000;

	public static void SetControl(DisplayControl control) {
		ushort data = 0x0000;
		data |= (ushort)(control.BGMode & 0b111);
		data |= (ushort)((control.ScreenDisplayBG2 ? 1 : 0) << 10);
		*DISPCNT = data;
	}

	public static void SetPixelRed(int x, int y) {
    	ushort* topLeftPixel = (ushort*)0x06000000;
    	*(topLeftPixel+x+(y*240)) = 0x001F; // Red
	}
}
