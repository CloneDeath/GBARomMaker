namespace GBA;

public unsafe static class CharacterData {
	private static readonly ushort* OBJ_TILES = (ushort*)0x06010000;
	//private static readonly ushort* OBJ_TILES = (ushort*)0x06014000;

	public static void SetColor(int character, byte x, byte y, int colorIndex) {
		var tileStart = OBJ_TILES + (character * 16);
		var wordOffset = (y * 2) + (x / 4);
		var existing = tileStart[wordOffset];

		int bitOffset = (x % 4)*4;
		ushort newColor = (ushort)(existing & ~(0xF << bitOffset));
		newColor |= (ushort)((colorIndex & 0xF) << bitOffset);
		tileStart[wordOffset] = newColor;
	}
}
