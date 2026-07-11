using System;

namespace GBARomMaker.Rom.Operations;

// https://problemkaputt.de/gbatek-arm-opcodes-data-processing-alu.htm
public class Or : Operation {
	public Or() {
		Condition = Condition.Always;
		Immediate = true;
		SetConditionCodes = false;
		FirstOperandRegister = 0;
		DestinationRegister = 0;
		ImmediateValue = 0;
	}

	public Or(byte[] data) {
		Condition = (Condition)(data[3] >> 4);
		if (((data[3] >> 2) & 0b11) != 0b00) {
			throw new Exception("Invalid data for a ORR instruction");
		}
		Immediate = ((data[3] >> 1) & 0b1) == 1;
		
		// OpCode check...
		var opcode = ((data[3] & 0b1) << 3) | (data[2] >> 5);
		if (opcode != 0b1100) {
			throw new Exception("Unexpected opcode for ORR");
		}

		SetConditionCodes = ((data[2] >> 4) & 0b1) == 1;
		FirstOperandRegister = (byte)(data[2] & 0b1111);
		DestinationRegister = (byte)(data[1] >> 4);

		var ror = (data[1] & 0b1111) * 2;
		var nn = (uint)(data[0]);
		ImmediateValue = this.RollRight(ror, nn);
	}

	public Condition Condition { get; set; }
	public Instruction Instruction { get; } = Instruction.Move;
	public bool Immediate { get; set; }
	public bool SetConditionCodes { get; set; }
	public byte FirstOperandRegister { get; set; }
	public byte DestinationRegister { get; set; }
	public uint ImmediateValue { get; set; }

	public override byte[] ToBytes() {
		var data = new byte[4] { 0, 0, 0, 0 };
		data[3] |= (byte)(((byte)Condition << 4) & 0b11110000);
		// instruction is 00, which is already set for data[3]...
		data[3] |= (byte)(Immediate ? 0b10 : 0b00);

		// opcode
		data[3] |= 0b1;
		data[2] |= 0b100 << 5;

		data[2] |= (byte)((SetConditionCodes ? 1 : 0) << 4);
		data[2] |= (byte)(FirstOperandRegister & 0b1111);
		
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
			: (nn >> ror) | (nn << (32-ror));
	}
	
	public uint RollLeft(int rol, uint nn) {
		return rol == 0
			? nn
			: (nn << rol) | (nn >> (32-rol));
	}
}
