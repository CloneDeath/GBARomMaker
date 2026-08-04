using System;
using System.Collections.Generic;
using System.Reflection.Emit;
using System.Reflection.Metadata;
using GBARomMaker.CIL;

namespace GBARomMaker.CILParse.Instructions;

// III.3.27 conv.<to type> – data conversion
public class CONV_R : CILInstruction {
	public static CILInstructionDefinition[] Definitions = [
		new(0x6B, 0, (_) => new CONV_R(4)),
		new(0x6C, 0, (_) => new CONV_R(8)),
	];

	public CONV_R(int bytes) {
		Bytes = bytes;
	}

	public int Bytes { get; }

	public OpCode OpCode => Bytes switch {
		4 => OpCodes.Conv_R4,
		8 => OpCodes.Conv_R8,
		_ => throw new Exception("Invalid number of bytes")
	};

    public byte[] GetBytes() {
		return [Bytes switch {
			4 => 0x6B,
			8 => 0x6C,
			_ => throw new Exception("Invalid number of bytes")
		}];
    }

    public string GetCIL(CILFactory factory, ICILMethod method) {
		return $"conv.r{Bytes}";
    }

	public void ModifyStack(CILFactory factory, ICILMethod method, Stack<SignatureTypeCode> current) {
		current.Pop();
		current.Push(Bytes switch {
			4 => SignatureTypeCode.Single,
			8 => SignatureTypeCode.Double,
			_ => throw new Exception("Invalid number of bytes")
		});
	}

    public bool AlwaysBranches => false;
	public bool SometimesBranches => false;
	public int? BranchTarget => null;
}
