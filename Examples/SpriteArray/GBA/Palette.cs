namespace GBA;

public static unsafe class Palette {
	private static readonly ushort* BG_Palette = (ushort*)0x05000000;
	private static readonly ushort* OBJ_Palette = (ushort*)0x05000200;

	public static void SetBackground(byte palette, byte index, Color color) {
		*(BG_Palette + (palette * 16) + index) = color.ToUShort();
	}

	public static void SetObject(byte palette, byte index, Color color) {
		*(OBJ_Palette + (palette * 16) + index) = color.ToUShort();
	}
}
