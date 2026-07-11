using System;

namespace GBARomMaker.Rom.Operations;

public class RORNN {
	public byte ROR;
	public byte nn;
}

// https://problemkaputt.de/gbatek-arm-opcodes-data-processing-alu.htm
public class Move : Operation {
	public Move() {
		Condition = Condition.Always;
		Immediate = true;
		SetConditionCodes = false;
		DestinationRegister = 0;
		ImmediateValue = 0;
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

		// First Operand Register check...
		var operand1 = data[2] & 0b1111;
		if (operand1 != 0b0000) throw new Exception("Expected 1st operand to be 0");

		DestinationRegister = (byte)(data[1] >> 4);

		var ror = (data[1] & 0b1111) * 2;
		var nn = (uint)(data[0]);
		ImmediateValue = this.RollRight(ror, nn);
	}

	public Condition Condition { get; set; }
	public Instruction Instruction { get; } = Instruction.Move;
	public bool Immediate { get; set; }
	public bool SetConditionCodes { get; set; }
	public byte DestinationRegister { get; set; }
	public uint ImmediateValue { get; set; }

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

		var rornn = this.calculateRORNN();
		data[1] |= (byte)((rornn.ROR/2) & 0b1111);
		data[0] |= rornn.nn;

		return data;
	}

	public RORNN calculateRORNN() {
		for (var i = 0; i <= 30; i += 2) {
			var nn = this.RollLeft(i, ImmediateValue);
			if (nn < 0x100) {
				return new RORNN {
					ROR = (byte)i,
					nn = (byte)nn 
				};
			}
		}
		throw new Exception("No valid ROR could be found");
	}

	public uint RollRight(int ror, uint nn) {
		return ror == 0
			? nn
			: (nn >> ror) | (nn << (31-ror));
	}
	
	public uint RollLeft(int rol, uint nn) {
		return rol == 0
			? nn
			: (nn << rol) | (nn >> (31-rol));
	}
}
