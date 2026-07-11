namespace GBARomMaker;

public class OpCode(byte[] data) {
	public Condition Condition {
		get => (Condition)(data[3] >> 4);
	}

	public InstructionType InstructionType {
		get => (InstructionType)((data[3] >> 1) & 0b111);
	}

	public Instruction Instruction {
		get {
			var opcode = data[3] & 1;
			return opcode == 0
				? Instruction.Branch
				: Instruction.BranchLink;
		}
	}

	public int Offset {
		get {
			int value = data[0] | (data[1] << 8) | (data[2] << 16);
			// Sign-extend bit 23 through the upper 8 bits.
			if ((value & 0x0080_0000) != 0)
				value |= unchecked((int)0xFF00_0000);
			return value * 4;
		}
	}

	public override string ToString() {
		var result = Instruction.ToString();
		if (Condition != Condition.Always) {
			result += Condition.ToString();
		}
		result += $" 0x{Offset.ToString("X6")}";
		return result;
	}
}
