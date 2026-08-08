using System;
using System.Collections.Generic;
using System.Reflection.Emit;
using System.Reflection.Metadata;
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

    public string GetCIL(CILFactory factory, ICILMethod method) {
		return $"conv.u{Bytes}";
    }

	public void ModifyStack(CILFactory factory, ICILMethod method, Stack<ISignatureType> current) {
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
