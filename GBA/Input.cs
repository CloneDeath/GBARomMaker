namespace GBA;

public enum KeyInput {
	A = 1 << 0,
	B = 1 << 1,
	Select = 1 << 2,
	Start = 1 << 3,
	Right = 1 << 4,
	Left = 1 << 5,
	Up = 1 << 6,
	Down = 1 << 7,
	R = 1 << 8,
	L = 1 << 9
}

public static unsafe class Input {
	private static ushort* KEYINPUT = (ushort*)0x4000130;

	public static KeyInput Pressed => (KeyInput)(~(*KEYINPUT));

	public static bool A => Pressed.HasFlag(KeyInput.A);
	public static bool B => Pressed.HasFlag(KeyInput.B);
	public static bool Select => Pressed.HasFlag(KeyInput.Select);
	public static bool Start => Pressed.HasFlag(KeyInput.Start);
	public static bool Right => Pressed.HasFlag(KeyInput.Right);
	public static bool Left => Pressed.HasFlag(KeyInput.Left);
	public static bool Up => Pressed.HasFlag(KeyInput.Up);
	public static bool Down => Pressed.HasFlag(KeyInput.Down);
	public static bool R => Pressed.HasFlag(KeyInput.R);
	public static bool L => Pressed.HasFlag(KeyInput.L);
}
