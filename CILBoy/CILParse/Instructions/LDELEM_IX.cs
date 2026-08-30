using System;
using System.Collections.Generic;
using System.Reflection.Emit;
using System.Reflection.Metadata;
using CILBoy.CIL;

namespace CILBoy.CILParse.Instructions;

public class LDELEM_IX : CILInstruction {
	public static CILInstructionDefinition[] Definitions = [
		new(0x90, 0, (_) => new LDELEM_IX(1)),
		new(0x92, 0, (_) => new LDELEM_IX(2)),
		new(0x94, 0, (_) => new LDELEM_IX(4)),
		new(0x96, 0, (_) => new LDELEM_IX(8))
	];

	public LDELEM_IX(int bytes) {
		Bytes = bytes;
	}

	public int Bytes { get; }

	public OpCode OpCode => Bytes switch {
		1 => OpCodes.Ldelem_I1,
		2 => OpCodes.Ldelem_I2,
		4 => OpCodes.Ldelem_I4,
		8 => OpCodes.Ldelem_I8,
		_ => throw new Exception("Invalid number of bytes")
	};

    public byte[] GetBytes() {
		return [Bytes switch {
			1 => 0x90,
			2 => 0x92,
			4 => 0x94,
			8 => 0x96,
			_ => throw new Exception("Invalid number of bytes")
		}];
    }

    public string GetCIL(ICILMethod method) {
		return $"ldelem.i{Bytes}";
    }

	public void ModifyStack(ICILMethod method, Stack<ISignatureType> current) {
		current.Pop();
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
