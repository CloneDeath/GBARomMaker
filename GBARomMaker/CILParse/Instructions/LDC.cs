using System;
using System.Linq;
using System.Reflection.Emit;

namespace GBARomMaker.CILParse.Instructions;

public static class LDC {
	public static CILInstructionDefinition[] Definitions = [
		new(0x1F, 1, (data) => new LDC_I4_S(data[0])), // ldc.i4.s
		new(0x20, 4, (data) => new LDC_I4(BitConverter.ToInt32(data))), // ldc.i4
	];
}

public class LDC_I4 : CILInstruction {
	public int Data { get; }
	public LDC_I4(int data) { this.Data = data; }

	public override OpCode OpCode => OpCodes.Ldc_I4;

	public override byte[] GetBytes() {
		return new byte[] { 0x20 }.Concat(BitConverter.GetBytes(Data)).ToArray();
	}

	public override string GetCIL() {
		return $"ldc.i4 0x{Data:X8}";
	}
}

public class LDC_I4_S : CILInstruction {
	public byte Data { get; }
	public LDC_I4_S(byte data) { this.Data = data; }
	
	public override OpCode OpCode => OpCodes.Ldc_I4_S;

	public override byte[] GetBytes() {
		return [0x1F, Data];
	}

	public override string GetCIL() {
		return $"ldc.i4.s 0x{Data:X2}";
	}
}

