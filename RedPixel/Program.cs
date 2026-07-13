//unsafe
//{
//    ushort* displayControl = (ushort*)0x04000000;
//    *displayControl = 0x0403; // Mode 3 + BG2 enabled
//
//    ushort* topLeftPixel = (ushort*)0x06000000;
//    *topLeftPixel = 0x001F; // Red
//}

unsafe
{
    *(ushort*)0x04000000 = 0x0403;
    *(ushort*)0x06000000 = 0x001F;
}
