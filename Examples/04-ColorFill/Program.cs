using GBA;

unsafe
{
	DisplayController.SetControl(new DisplayControl {
		BGMode = 3,
		ScreenDisplayBG2 = true,
	});

	for (var v = 0; v < 5; v++) {
	for (var h = 0; h < 6; h++) {
		for (var y = 0; y < 32; y++) {
		for (var x = 0; x < 32; x++){
			var color = new Color {
				Red = (byte)x,
				Blue = (byte)y,
				Green = (byte)(h + (v * 6))
			};
			DisplayController.SetPixel(x + (h*32) + 8 + 16, y + (v*32), color);
		}
		}
	}
	}

	while(true){};
}
