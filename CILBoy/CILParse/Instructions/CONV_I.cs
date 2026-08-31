using System;
using System.Collections.Generic;
using System.Reflection.Emit;
using System.Reflection.Metadata;
using CILBoy.CIL;

namespace CILBoy.CILParse.Instructions;

public class CONV_I : CILInstruction {
	public static CILInstructionDefinition[] Definitions = [
		new(0x67, 0, (_) => new CONV_I(1)),
		new(0x68, 0, (_) => new CONV_I(2)),
		new(0x69, 0, (_) => new CONV_I(4)),
		new(0x6A, 0, (_) => new CONV_I(8)),
	];

	public CONV_I(int bytes) {
		Bytes = bytes;
	}

	public int Bytes { get; }

	public OpCode OpCode => Bytes switch {
		1 => OpCodes.Conv_I1,
		2 => OpCodes.Conv_I2,
		4 => OpCodes.Conv_I4,
		8 => OpCodes.Conv_I8,
		_ => throw new Exception("Invalid number of bytes")
	};

    public byte[] GetBytes() {
		return [Bytes switch {
			1 => 0x67,
			2 => 0x68,
			4 => 0x69,
			8 => 0x6A,
			_ => throw new Exception("Invalid number of bytes")
		}];
    }

    public string GetCIL(CILMethodDefinition method) {
		return $"conv.i{Bytes}";
    }

	public void ModifyStack(CILMethodDefinition method, Stack<ISignatureType> current) {
		current.Pop();
		current.Push(new SignatureType(Bytes switch {
			1 => SignatureTypeCode.Byte,
			2 => SignatureTypeCode.Int16,
			4 => SignatureTypeCode.Int32,
			8 => SignatureTypeCode.Int64,
			_ => throw new Exception("Invalid number of bytes")
		}));
	}

    public bool AlwaysBranches => false;
	public bool SometimesBranches => false;
	public int? BranchTarget => null;
}
