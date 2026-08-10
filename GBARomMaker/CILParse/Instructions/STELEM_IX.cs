using System;
using System.Collections.Generic;
using System.Reflection.Emit;
using System.Reflection.Metadata;
using GBARomMaker.CIL;

namespace GBARomMaker.CILParse.Instructions;

public class STELEM_IX : CILInstruction {
	public static CILInstructionDefinition[] Definitions = [
		new(0x9C, 0, (_) => new STELEM_IX(1)),
		new(0x9D, 0, (_) => new STELEM_IX(2)),
		new(0x9E, 0, (_) => new STELEM_IX(4)),
		new(0x9F, 0, (_) => new STELEM_IX(8))
	];

	public STELEM_IX(int bytes) {
		Bytes = bytes;
	}

	public int Bytes { get; }

	public OpCode OpCode => Bytes switch {
		1 => OpCodes.Stelem_I1,
		2 => OpCodes.Stelem_I2,
		4 => OpCodes.Stelem_I4,
		8 => OpCodes.Stelem_I8,
		_ => throw new Exception("Invalid number of bytes")
	};

    public byte[] GetBytes() {
		return [Bytes switch {
			1 => 0x9C,
			2 => 0x9D,
			4 => 0x9E,
			8 => 0x9F,
			_ => throw new Exception("Invalid number of bytes")
		}];
    }

    public string GetCIL(CILFactory factory, ICILMethod method) {
		return $"stelem.i{Bytes}";
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
