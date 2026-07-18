namespace GBARomMaker.ARM;

public enum MULOperation {
	MUL = 0b0000,
	MLA = 0b0001,
	UMAAL = 0b0010,
	UMULL = 0b0100,
	UMLAL = 0b0101,
	SMULL = 0b0110,
	SMLAL = 0b0111,
	SMLAxy = 0b1000,
	SMLAWy = 0b1001,
	SMULWy = 0b1001,
	SMLALxy = 0b1010,
	SMULxy = 0b1011,
}

// https://problemkaputt.de/gbatek-arm-opcodes-multiply-and-multiply-accumulate-mul-mla.htm
public class Multiply : IInstruction {
	public Multiply() {
		Condition = Condition.Always;
		Opcode = MULOperation.MUL;
		SetConditionCodes = false;
		DestinationRegister = 0;
		AccumulateRegister = 0;
		Op2Register = 0;
		Op1Register = 0;
	}

	public Condition Condition { get; set; }
	public required MULOperation Opcode { get; set; }
	public bool SetConditionCodes { get; set; }
	public byte DestinationRegister { get; set; }
	public byte AccumulateRegister { get; set; }
	public byte Op2Register { get; set; }
	public byte Op1Register { get; set; }
	
	public byte[] ToBytes() {
		var data = new byte[4] { 0, 0, 0, 0 };
		data[3] |= (byte)(((byte)Condition << 4) & 0b11110000);
		// [27-25] = 0b000
		data[3] |= (byte)(((byte)Opcode & 0b1111) >> 3);
		data[2] |= (byte)((byte)Opcode << 5);
		data[2] |= (byte)((SetConditionCodes ? 1 : 0) << 4);
		data[2] |= (byte)(DestinationRegister & 0b1111);
		data[1] |= (byte)((AccumulateRegister & 0b1111) << 4);
		data[1] |= (byte)(Op2Register & 0b1111);

		// TODO implement halfword multiplies
		data[0] |= (byte)(0b1001 << 4);

		data[0] |= (byte)(Op1Register & 0b1111);
		return data;
	}
}
