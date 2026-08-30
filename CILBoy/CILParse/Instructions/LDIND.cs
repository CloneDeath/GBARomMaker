using System;
using System.Collections.Generic;
using System.Reflection.Emit;
using System.Reflection.Metadata;
using CILBoy.CIL;

namespace CILBoy.CILParse.Instructions;

public static class LDIND {
	public static CILInstructionDefinition[] Definitions = [
		new(0x47, 0, (_) => new LDIND_U(1)),
		new(0x49, 0, (_) => new LDIND_U(2)),
		new(0x4B, 0, (_) => new LDIND_U(4)),
	];
}

public class LDIND_U : CILInstruction {
	public LDIND_U(int bytes) {
		Bytes = bytes;
	}

	public int Bytes { get; set; }

	public OpCode OpCode {
		get {
			return Bytes switch {
				1 => OpCodes.Ldind_U1,
				2 => OpCodes.Ldind_U2,
				4 => OpCodes.Ldind_U4,
				_ => throw new NotSupportedException("No valid opcode for value " + Bytes)
			};
		}
	}

	public byte[] GetBytes() {
		return [Bytes switch {
			1 => 0x47,
			2 => 0x49,
			4 => 0x4B,
			_ => throw new NotSupportedException("No valid opcode for value " + Bytes)
		}];
	}

    public string GetCIL(ICILMethod method) {
		return $"ldind.u{Bytes}";
    }
    
	public void ModifyStack(ICILMethod method, Stack<ISignatureType> current) {
		if (Bytes == 8) {
			current.Push(new SignatureType(SignatureTypeCode.Int64));
		} else {
			current.Push(new SignatureType(SignatureTypeCode.Int32));
		}
	}

    public bool AlwaysBranches => false;
	public bool SometimesBranches => false;
	public int? BranchTarget => null;
}
