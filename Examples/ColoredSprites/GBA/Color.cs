namespace GBA;

public class Color {
	public byte Red;
	public byte Green;
	public byte Blue;
}

public static class Colors {
	public static readonly Color Red = new Color { Red = 31 };
	public static readonly Color Green = new Color { Green = 31 };
	public static readonly Color Blue = new Color { Blue = 31 };
}
