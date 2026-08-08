using System.Runtime.InteropServices;

namespace GBA;

public static class Interrupt {
    [DllImport("gba", EntryPoint = "WaitVBlank")]
	public static extern void WaitVBlank();
}
