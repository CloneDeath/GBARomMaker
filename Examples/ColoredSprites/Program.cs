unsafe
{
    ushort* displayControl = (ushort*)0x04000000;
    *displayControl = 0x0403; // Mode 3 + BG2 enabled

    ushort* topLeftPixel = (ushort*)0x06000000;
	for (var i = 0; i < 10; i++) {
    	*(topLeftPixel+i) = 0x001F; // Red
	}
}
