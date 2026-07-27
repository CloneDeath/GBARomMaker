unsafe
{
	DisplayController.SetControl(new DisplayControl {
		BGMode = 3,
		ScreenDisplayBG2 = true,
	});

	var color = new Color { Red = 31 };
	for (var x = 0; x < 240; x++){
		//for (var y = 0; y < 1; y++) {
			color.Red += 1;
			if (color.Red >= 32) {
				color.Red = 0;
				color.Blue += 1;
			}
			DisplayController.SetPixelRed(x, 1, color);
		//}
	}

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

public class Color {
	public byte Red;
	public byte Green;
	public byte Blue;
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

	public static void SetPixelRed(int x, int y, Color color) {
    	ushort* topLeftPixel = (ushort*)0x06000000;
		ushort* destination = topLeftPixel+x+(y*240);
		ushort value = 0x0000;
    	value = (ushort)(color.Red & 0x001F);
    	//value |= 0x03E0; // Green
    	value |= (ushort)((color.Blue & 0x001F) << 10);
		*destination = value;
	}
}
