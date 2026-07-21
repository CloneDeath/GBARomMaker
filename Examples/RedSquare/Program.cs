unsafe
{
	DisplayController.EnableMode3AndBG2();

	for (var i = 0; i <= 10; i++) {
		DisplayController.SetPixelRed(i, 0);
		DisplayController.SetPixelRed(i, 10);
		DisplayController.SetPixelRed(0, i);
		DisplayController.SetPixelRed(10, i);
	}
}

public static unsafe class DisplayController {
	public static void EnableMode3AndBG2() {
		ushort* displayControl = (ushort*)0x04000000;
		*displayControl = 0x0403; // Mode 3 + BG2 enabled
	}

	public static void SetPixelRed(int x, int y) {
		ushort* topLeftPixel = (ushort*)0x06000000;
		*(topLeftPixel+x+(y*240)) = 0x001F; // Red
	}
}