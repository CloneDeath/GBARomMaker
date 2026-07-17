using System;
using GBARomMaker.Rom.Operations.ALU;

namespace GBARomMaker.Rom.Operations;

public class RORNN {
	public byte ROR;
	public byte nn;
}

// https://problemkaputt.de/gbatek-arm-opcodes-data-processing-alu.htm
public class Move : IOperation {
	public Move() {
		Condition = Condition.Always;
		SetConditionCodes = false;
		DestinationRegister = 0;
		Op2 = new Immediate(0);
	}

	public Move(byte[] data) {
		Condition = (Condition)(data[3] >> 4);
		if (((data[3] >> 2) & 0b11) != 0b00) {
			throw new Exception("Invalid data for a MOV instruction");
		}
		var immediate = ((data[3] >> 1) & 0b1) == 1;
		
		// OpCode check...
		var opcode = ((data[3] & 0b1) << 3) | (data[2] >> 5);
		if (opcode != 0b1101) {
			throw new Exception("Unexpected opcode for MOV");
		}

		SetConditionCodes = ((data[2] >> 4) & 0b1) == 1;

		// First Operand Register check...
		var operand1 = data[2] & 0b1111;
		if (operand1 != 0b0000) throw new Exception("Expected 1st operand to be 0");

		DestinationRegister = (byte)(data[1] >> 4);

		Op2 = immediate
			? new Immediate(data[0..1])
			: throw new NotImplementedException();
	}

	public Condition Condition { get; set; }
	public Instruction Instruction { get; } = Instruction.Move;
	public bool Immediate => Op2.IsImmediate;
	public bool SetConditionCodes { get; set; }
	public byte DestinationRegister { get; set; }
	public ALUOp2 Op2 { get; set; }

	public override byte[] ToBytes() {
		var data = new byte[4] { 0, 0, 0, 0 };
		data[3] |= (byte)(((byte)Condition << 4) & 0b11110000);
		// instruction is 00, which is already set for data[3]...
		data[3] |= (byte)(Immediate ? 0b10 : 0b00);

		// opcode
		data[3] |= 0b1;
		data[2] |= 0b101 << 5;

		data[2] |= (byte)((SetConditionCodes ? 1 : 0) << 4);
		// first operand is always 0, which is already set for data[2]...
		
		data[1] |= (byte)(DestinationRegister << 4);

		var op2data = Op2.ToBytes();
		data[1] |= (byte)(op2data[1] & 0b1111);
		data[0] = op2data[0];

		return data;
	}
}
