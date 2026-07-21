using System;
using GBARomMaker.ARM.ALU;
using GBARomMaker.ARM.Common;

namespace GBARomMaker.ARM;

public enum ALUOperation {
	AND = 0x0,
	EOR = 0x1,
	SUB = 0x2,
	RSB = 0x3,
	ADD = 0x4,
	ADC = 0x5,
	SBC = 0x6,
	RSC = 0x7,
	TST = 0x8,
	TEQ = 0x9,
	CMP = 0xA,
	CMN = 0xB,
	ORR = 0xC,
	MOV = 0xD,
	BIC = 0xE,
	MVN = 0xF
}

public class RORNN {
	public byte ROR;
	public byte nn;
}

// https://problemkaputt.de/gbatek-arm-opcodes-data-processing-alu.htm
public class DataProcessing: IInstruction {
	public DataProcessing() {
		Condition = Condition.Always;
		Operation = ALUOperation.MOV;
		SetConditionCodes = false;
		Op1Register = 0;
		DestinationRegister = 0;
		Op2 = new Immediate(0);
	}

	public DataProcessing(byte[] data) {
		Condition = (Condition)(data[3] >> 4);
		if (((data[3] >> 2) & 0b11) != 0b00) {
			throw new Exception("Invalid data for a MOV instruction");
		}
		var immediate = ((data[3] >> 1) & 0b1) == 1;
		
		var opcode = ((data[3] & 0b1) << 3) | (data[2] >> 5);
		Operation = (ALUOperation)opcode;

		SetConditionCodes = ((data[2] >> 4) & 0b1) == 1;
		Op1Register = (byte)(data[2] & 0b1111);
		DestinationRegister = (byte)(data[1] >> 4);

		Op2 = immediate
			? new Immediate(data[0..1])
			: throw new NotImplementedException();
	}

	public Condition Condition { get; set; }
	public bool Immediate => Op2.IsImmediate;
	public required ALUOperation Operation { get; set; }
	public bool SetConditionCodes { get; set; }
	public byte Op1Register { get; set; }
	public byte DestinationRegister { get; set; }
	public ALUOp2 Op2 { get; set; }

	public byte[] ToBytes() {
		var isCompareOp = Operation == ALUOperation.TST
			|| Operation == ALUOperation.TEQ
			|| Operation == ALUOperation.CMP
			|| Operation == ALUOperation.CMN;

		if (Op1Register != 0b0000 && (Operation == ALUOperation.MOV || Operation == ALUOperation.MVN)) throw new Exception("Expected 1st operand to be 0");
		if (DestinationRegister != 0 && isCompareOp) throw new Exception("Destination Register must be 0 for Operation " + Operation);

		var data = new byte[4] { 0, 0, 0, 0 };
		data[3] |= (byte)(((byte)Condition << 4) & 0b11110000);
		// instruction has 00 for [27-26]
		data[3] |= (byte)((Immediate ? 1 : 0) << 1);

		data[3] |= (byte)(((byte)Operation >> 3) & 0b1);
		data[2] |= (byte)(((byte)Operation & 0b111) << 5);

		// must be true for any of these operations
		var setConditionCodes = SetConditionCodes || isCompareOp ;
		data[2] |= (byte)((SetConditionCodes ? 1 : 0) << 4);
		data[2] |= (byte)(Op1Register & 0b1111);
		data[1] |= (byte)(DestinationRegister << 4);

		var op2data = Op2.ToBytes();
		data[1] |= (byte)(op2data[1] & 0b1111);
		data[0] = op2data[0];

		return data;
	}
}
