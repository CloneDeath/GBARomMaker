using System;
using GBARomMaker.ARM.Common;

namespace GBARomMaker.ARM.ALU;

public class Register : ALUOp2 {
	public bool IsImmediate => false;

	public Register() : this(0) {}

	public Register(byte register) {
		OpRegister = register;
		ShiftByRegister = false;
		ShiftType = ShiftType.LogicalShiftLeft;
		ShiftAmount = 0;
	}

	public Register(byte[] data) {
		OpRegister = (byte)(data[0] & 0b1111);
		ShiftByRegister = ((data[0] >> 4) & 0b1) == 1;
		ShiftType = (ShiftType)((data[0] >> 5) & 0b11);

		if (ShiftByRegister) {
			if (((data[0] >> 7) & 0b1) != 1) throw new Exception("Reserved bit must be 1");
			ShiftRegister = (byte)(data[1] & 0b1111);
		} else {
			ShiftAmount = (byte)(((data[1] & 0b1111) << 1) & (data[0] >> 7));
		}
	}

	public byte OpRegister { get; set; }
	public bool ShiftByRegister { get; }
	public ShiftType ShiftType { get; set; }

	public byte ShiftAmount { get; set; }
	public byte ShiftRegister { get; set; }

	public byte[] ToBytes() {
		var data = new byte[2] { 0, 0 };
		data[1] = ShiftByRegister
			? (byte)((ShiftRegister) & 0b1111)
			: (byte)((ShiftAmount >> 1) & 0b1111);
		data[0] |= (byte)((ShiftByRegister ? 1 : ShiftAmount) << 7);
		data[0] |= (byte)(((byte)ShiftType & 0b11) << 5);
		data[0] |= (byte)(ShiftByRegister ? 0b10000 : 0);
		data[0] |= (byte)(OpRegister & 0b1111);
		return data;
	}
}
