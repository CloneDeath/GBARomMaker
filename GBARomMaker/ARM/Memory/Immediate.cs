namespace GBARomMaker.ARM.Memory;

public class Immediate : IOffset {
	public bool IsImmediate => true;

	public uint Value { get; set; }

	public Immediate() {
		Value = 0;
	}

	public Immediate(uint value) {
		Value = value;
	}

	public byte[] ToBytes() {
		var data = new byte[2] { 0, 0 };
		data[1] = (byte)((Value >> 4) & 0b1111);
		data[0] = (byte)(Value & 0xFF);
		return data;
	}
}
