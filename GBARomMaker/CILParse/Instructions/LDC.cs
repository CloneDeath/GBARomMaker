using System;
using GBARomMaker.CIL;
using System.Linq;
using System.Reflection.Emit;

namespace GBARomMaker.CILParse.Instructions;

public static class LDC {
	public static CILInstructionDefinition[] Definitions = [
		new(0x15, 0, (data) => new LDC_I4_X(-1)), // ldc.i4.m1
		new(0x16, 0, (data) => new LDC_I4_X(0)), // ldc.i4.0
		new(0x17, 0, (data) => new LDC_I4_X(1)), // ldc.i4.1
		new(0x18, 0, (data) => new LDC_I4_X(2)), // ldc.i4.2
		new(0x19, 0, (data) => new LDC_I4_X(3)), // ldc.i4.3
		new(0x1A, 0, (data) => new LDC_I4_X(4)), // ldc.i4.4
		new(0x1B, 0, (data) => new LDC_I4_X(5)), // ldc.i4.5
		new(0x1C, 0, (data) => new LDC_I4_X(6)), // ldc.i4.6
		new(0x1D, 0, (data) => new LDC_I4_X(7)), // ldc.i4.7
		new(0x1E, 0, (data) => new LDC_I4_X(8)), // ldc.i4.8
		new(0x1F, 1, (data) => new LDC_I4_S(data[0])), // ldc.i4.s
		new(0x20, 4, (data) => new LDC_I4(BitConverter.ToInt32(data))), // ldc.i4
	];
}

public class LDC_I4 : CILInstruction {
	public int Data { get; }
	public LDC_I4(int data) { this.Data = data; }

	public OpCode OpCode => OpCodes.Ldc_I4;

	public byte[] GetBytes() {
		return new byte[] { 0x20 }.Concat(BitConverter.GetBytes(Data)).ToArray();
	}

	public string GetCIL(CILFactory factory) {
		return $"ldc.i4 0x{Data:X8}";
	}
}

public class LDC_I4_X : CILInstruction {
	public int Data { get; }
	public LDC_I4_X(int data) {
		if (data < -1 || data > 8) {
			throw new InvalidOperationException("Valid ldc.i4.x command only supports between -1 and 8");
		}
		this.Data = data;
	}

	public OpCode OpCode {
		get {
			return Data switch {
				-1 => OpCodes.Ldc_I4_M1,
				0 => OpCodes.Ldc_I4_0,
				1 => OpCodes.Ldc_I4_1,
				2 => OpCodes.Ldc_I4_2,
				3 => OpCodes.Ldc_I4_3,
				4 => OpCodes.Ldc_I4_4,
				5 => OpCodes.Ldc_I4_5,
				6 => OpCodes.Ldc_I4_6,
				7 => OpCodes.Ldc_I4_7,
				8 => OpCodes.Ldc_I4_8,
				_ => throw new NotSupportedException("No valid opcode for value " + Data)
			};
		}
	}

	public byte[] GetBytes() {
		return [Data switch {
			-1 => 0x15,
			0 => 0x16,
			1 => 0x17,
			2 => 0x18,
			3 => 0x19,
			4 => 0x1A,
			5 => 0x1B,
			6 => 0x1C,
			7 => 0x1D,
			8 => 0x1E,
			_ => throw new NotSupportedException("No valid opcode for value " + Data)
		}];
	}

	public string GetCIL(CILFactory factory) {
		var sign = Data == -1 ? "m1" : Data.ToString();
		return $"ldc.i4.{sign}";
	}
}

public class LDC_I4_S : CILInstruction {
	public byte Data { get; }
	public LDC_I4_S(byte data) { this.Data = data; }
	
	public OpCode OpCode => OpCodes.Ldc_I4_S;

	public byte[] GetBytes() {
		return [0x1F, Data];
	}

	public string GetCIL(CILFactory factory) {
		return $"ldc.i4.s 0x{Data:X2}";
	}
}

