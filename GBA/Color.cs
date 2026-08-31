using System;

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

	public static Color FromHSV(float hue, float saturation, float value) {
		int hi = Convert.ToInt32(MathF.Floor(hue / 60)) % 6;
		float f = hue / 60 - MathF.Floor(hue / 60);

		value = value * 31;
		byte v = Convert.ToByte(value);
		byte p = Convert.ToByte(value * (1 - saturation));
		byte q = Convert.ToByte(value * (1 - f * saturation));
		byte t = Convert.ToByte(value * (1 - (1 - f) * saturation));

		if (hi == 0)
			return new Color { Red = v, Green = t, Blue = p };
		else if (hi == 1)
			return new Color { Red = q, Green = v, Blue = p };
		else if (hi == 2)
			return new Color { Red = p, Green = v, Blue = t };
		else if (hi == 3)
			return new Color { Red = p, Green = q, Blue = v };
		else if (hi == 4)
			return new Color { Red = t, Green = p, Blue = v };
		else
			return new Color { Red = v, Green = p, Blue = q };
	}
}

public static class Colors {
	public static readonly Color Red = new Color { Red = 31 };
	public static readonly Color Green = new Color { Green = 31 };
	public static readonly Color Blue = new Color { Blue = 31 };
}
