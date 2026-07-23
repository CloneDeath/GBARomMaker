using System;
using System.Reflection.Emit;
using GBARomMaker.CIL;

namespace GBARomMaker.CILParse.Instructions;

public class CONV_U : CILInstruction {
	public static CILInstructionDefinition[] Definitions = [
		new(0xD2, 0, (_) => new CONV_U(1)),
		new(0xD1, 0, (_) => new CONV_U(2)),
		new(0x6D, 0, (_) => new CONV_U(4)),
		new(0x6E, 0, (_) => new CONV_U(8)),
	];

	public CONV_U(int bytes) {
		Bytes = bytes;
	}

	public int Bytes { get; }

	public OpCode OpCode => Bytes switch {
		1 => OpCodes.Conv_U1,
		2 => OpCodes.Conv_U2,
		4 => OpCodes.Conv_U4,
		8 => OpCodes.Conv_U8,
		_ => throw new Exception("Invalid number of bytes")
	};

    public byte[] GetBytes() {
		return [Bytes switch {
			1 => 0xD2,
			2 => 0xD1,
			4 => 0x6D,
			8 => 0x6E,
			_ => throw new Exception("Invalid number of bytes")
		}];
    }

    public string GetCIL(CILFactory factory) {
		return $"conv.u{Bytes}";
    }
}
