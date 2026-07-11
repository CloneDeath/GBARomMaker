using System;

namespace GBARomMaker.Rom.Operations;

// https://problemkaputt.de/gbatek-arm-opcodes-data-processing-alu.htm
public class Move : Operation {
	public Move() {
		Condition = Condition.Always;
		Immediate = true;
		SetConditionCodes = false;
	}

	public Move(byte[] data) {
		Condition = (Condition)(data[3] >> 4);
		if (((data[3] >> 2) & 0b11) != 0b00) {
			throw new Exception("Invalid data for a MOV instruction");
		}
		Immediate = ((data[3] >> 1) & 0b1) == 1;
		
		// OpCode check...
		var opcode = ((data[3] & 0b1) << 3) | (data[2] >> 5);
		if (opcode != 0b1101) {
			throw new Exception("Unexpected opcode for data");
		}

		SetConditionCodes = ((data[2] >> 4) & 0b1) == 1;
	}

	public Condition Condition { get; set; }
	public Instruction Instruction { get; } = Instruction.Move;
	public bool Immediate { get; set; }
	public bool SetConditionCodes { get; set; }

	public override byte[] ToBytes() {
		var data = new byte[4] { 0, 0, 0, 0 };
		data[3] |= (byte)(((byte)Condition << 4) & 0b11110000);
		// instruction is 00, which is already set for data[3]...
		data[3] |= (byte)(Immediate ? 0b10 : 0b00);

		// opcode
		data[3] |= 0b1;
		data[2] |= 0b101 << 5;

		data[2] |= (byte)((SetConditionCodes ? 1 : 0) << 4);

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
