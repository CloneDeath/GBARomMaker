using GBARomMaker.ARM.Common;

namespace GBARomMaker.ARM;

public class SoftwareInterrupt : IInstruction {
	public SoftwareInterrupt() {}

	public Condition Condition { get; set; }
	public uint Comment { get; set; }

    public byte[] ToBytes() {
		var data = new byte[4] { 0, 0, 0, 0 };
		data[3] |= (byte)(((byte)Condition << 4) & 0b11110000);
		data[3] |= 0b1111;

		data[2] = (byte)(Comment >> 16);
		data[1] = (byte)(Comment >> 8);
		data[0] = (byte)Comment;
		return data;
    }
}
