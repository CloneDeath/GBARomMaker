using System;
using GBARomMaker.ARM.Common;

namespace GBARomMaker.ARM;

// https://problemkaputt.de/gbatek-arm-opcodes-branch-and-branch-with-link-b-bl-bx-blx-swi-bkpt.htm
public class Branch : ILabeledInstruction {
	public Branch() {
		Condition = Condition.Always;
		Instruction = Instruction.Branch;
		Offset = 0x00;
	}

	public Branch(byte[] data) {
		Condition = (Condition)(data[3] >> 4);
		if (((data[3] >> 1) & 0b111) != 0b101) {
			throw new Exception("Invalid data for a branch instruction");
		}
		Instruction = (data[3] & 0b1) == 0
			? Instruction.Branch
			: Instruction.BranchLink;

		int offset = data[0] | (data[1] << 8) | (data[2] << 16);
		// Sign-extend bit 23 through the upper 8 bits.
		if ((offset & 0x0080_0000) != 0)
			offset |= unchecked((int)0xFF00_0000);
		Offset = (offset * 4)+8;
	}

	public Condition Condition { get; set; }
	public Instruction Instruction { get; set; }
	public int Offset { get; set; }

	public void SetOffset(int offset) {
		Offset = offset;
	}

	public byte[] ToBytes() {
		var data = new byte[4] { 0, 0, 0, 0 };
		data[3] |= (byte)(((byte)Condition << 4) & 0b11110000);
		data[3] |= (byte)(Instruction == Instruction.Branch 
			? 0b1010
			: 0b1011);

		var offset = (Offset - 8) / 4;
		data[0] = (byte)(offset & 0xFF);
		data[1] = (byte)((offset >> 8) & 0xFF);
		data[2] = (byte)((offset >> 16) & 0xFF);
		return data;
	}

	public override string ToString() {
		var result = Instruction.ToString();
		if (Condition != Condition.Always) {
			result += Condition.ToString();
		}
		result += $" +0x{Offset.ToString("X6")}";
		return result;
	}
}
