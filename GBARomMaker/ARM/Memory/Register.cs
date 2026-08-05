using GBARomMaker.ARM.Common;

namespace GBARomMaker.ARM.Memory;

public class Register : IOffset {
	public bool IsImmediate => false;

	public Register() {}

	public required byte ShiftAmount { get; init; }
	public required ShiftType ShiftType { get; init; }
	public required byte OffsetRegister { get; init; }

	public byte[] ToBytes() {
		var data = new byte[2] { 0, 0 };
		data[1] = (byte)((ShiftAmount >> 1) & 0b1111);
		data[0] = (byte)((ShiftAmount & 0b1) << 7);
		data[0] |= (byte)((byte)ShiftType << 5);
		data[0] |= (byte)(OffsetRegister & 0b1111);
		return data;
	}
}
