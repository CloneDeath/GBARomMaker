namespace GBARomMaker.ARM;

public enum BranchExchangeOpCode {
	BX = 0b0001,
	BXJ = 0b0010,
	BLX = 0b0011
}

// https://problemkaputt.de/gbatek-arm-opcodes-branch-and-branch-with-link-b-bl-bx-blx-swi-bkpt.htm
public class BranchExchange : IInstruction {
	public BranchExchange() {
		Condition = Condition.Always;
		OpCode = BranchExchangeOpCode.BX;
		Register = 14;
	}

	public Condition Condition { get; set; }
	public BranchExchangeOpCode OpCode { get; set; }
	public byte Register { get; set; }

    public byte[] ToBytes() {
		var data = new byte[4] { 0, 0, 0, 0 };
		data[3] |= (byte)(((byte)Condition << 4) & 0b11110000);
		data[3] |= 0b0001; 
		data[2] |= 0b0010_1111;
		data[1] |= 0b1111_1111;
		data[0] |= (byte)(((byte)OpCode & 0b1111) << 4);
		data[0] |= (byte)(Register & 0b1111);
		return data;
    }
}
