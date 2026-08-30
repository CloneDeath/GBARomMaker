using System;
using System.Collections.Generic;
using System.Reflection.Emit;
using System.Reflection.Metadata;
using CILBoy.CIL;

namespace CILBoy.CILParse.Instructions;

public class LDELEM_UX : CILInstruction {
	public static CILInstructionDefinition[] Definitions = [
		new(0x91, 0, (_) => new LDELEM_UX(1)),
		new(0x93, 0, (_) => new LDELEM_UX(2)),
		new(0x95, 0, (_) => new LDELEM_UX(4)),
		// new(0x96, 0, (_) => new LDELEM_UX(8)) ALIAS for ldelem.i8
	];

	public LDELEM_UX(int bytes) {
		Bytes = bytes;
	}

	public int Bytes { get; }

	public OpCode OpCode => Bytes switch {
		1 => OpCodes.Ldelem_U1,
		2 => OpCodes.Ldelem_U2,
		4 => OpCodes.Ldelem_U4,
		_ => throw new Exception("Invalid number of bytes")
	};

    public byte[] GetBytes() {
		return [Bytes switch {
			1 => 0x91,
			2 => 0x93,
			4 => 0x95,
			_ => throw new Exception("Invalid number of bytes")
		}];
    }

    public string GetCIL(ICILMethod method) {
		return $"ldelem.u{Bytes}";
    }

	public void ModifyStack(ICILMethod method, Stack<ISignatureType> current) {
		current.Pop();
		current.Pop();
		current.Push(new SignatureType(Bytes switch {
			1 => SignatureTypeCode.Byte,
			2 => SignatureTypeCode.UInt16,
			4 => SignatureTypeCode.UInt32,
			_ => throw new Exception("Invalid number of bytes")
		}));
	}

    public bool AlwaysBranches => false;
	public bool SometimesBranches => false;
	public int? BranchTarget => null;

}
