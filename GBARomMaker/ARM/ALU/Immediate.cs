using System;

namespace GBARomMaker.ARM.ALU;

public class Immediate : ALUOp2 {
	public override bool IsImmediate => true;
	public uint Value { get; set; }

	public Immediate() {
		Value = 0;
	}

	public Immediate(uint value) {
		Value = value;
	}

	public Immediate(byte[] data) {
		var ror = (data[1] & 0b1111) * 2;
		var nn = (uint)(data[0]);
		Value = this.RollRight(ror, nn);
	}

	public RORNN calculateRORNN() {
		for (var i = 0; i <= 30; i += 2) {
			var nn = this.RollLeft(i, Value);
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

	public override byte[] ToBytes() {
		var data = new byte[2] { 0, 0 };
		var rornn = this.calculateRORNN();
		data[1] |= (byte)((rornn.ROR/2) & 0b1111);
		data[0] |= rornn.nn;
		return data;
	}
}

