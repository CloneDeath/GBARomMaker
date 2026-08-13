namespace GBA;

public static unsafe class Palette {
	private static readonly ushort* BG_Palette = (ushort*)0x05000000;
	private static readonly ushort* OBJ_Palette = (ushort*)0x05000200;

	public static void SetBackground(byte paletteIndex, byte colorIndex, Color color) {
		*(BG_Palette + (paletteIndex * 16) + colorIndex) = color.ToUShort();
	}

	public static void SetObject(byte paletteIndex, byte colorIndex, Color color) {
		*(OBJ_Palette + (paletteIndex * 16) + colorIndex) = color.ToUShort();
	}
}
