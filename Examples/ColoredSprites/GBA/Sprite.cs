namespace GBA;

public unsafe class Sprite {
	private static readonly ushort* OAM = (ushort*)0x07000000;
	private readonly byte _index;

	public Sprite(byte index) {
		_index = index;
	}

	private ushort* current => OAM + (_index*4);

	public ushort X {
		get {
    		return (byte)(current[1] & 0x01FF);
		}
		set {
			var existing = current[1];
			existing &= 0xFE00;
			existing |= (ushort)(value & 0x01FF);
    		current[1] = existing;
		}
	}

	public byte Y {
		get {
    		return (byte)(current[0] & 0xFF);
		}
		set {
			var existing = current[0];
			existing &= 0xFF00;
			existing |= (byte)(value & 0xFF);
    		current[0] = existing;
		}
	}

	public ushort TileIndex {
		get {
			return (ushort)(current[2] & 0x03FF);
		}
		set {
			var existing = current[2];
			existing &= 0xFC00;
			existing |= (ushort)(value & 0x03FF);
    		current[2] = existing;
		}
	}
}
