namespace GBA;

public class Color {
	public byte Red;
	public byte Green;
	public byte Blue;

	public ushort ToUShort() {
		ushort value = 0x0000;
    	value = (ushort)(Red & 0x001F);
    	value |= (ushort)((Green & 0x001F) << 5);
    	value |= (ushort)((Blue & 0x001F) << 10);
		return value;
	}
}

public static class Colors {
	public static readonly Color Red = new Color { Red = 31 };
	public static readonly Color Green = new Color { Green = 31 };
	public static readonly Color Blue = new Color { Blue = 31 };
}
