using System;
using System.Collections.Generic;
using System.Reflection.Emit;
using CILBoy.CIL;

namespace CILBoy.CILParse.Instructions;

public class STIND_IX : CILInstruction {
	public static CILInstructionDefinition[] Definitions = [
		new(0x52, 0, (_) => new STIND_IX(1)),
		new(0x53, 0, (_) => new STIND_IX(2)),
		new(0x54, 0, (_) => new STIND_IX(4)),
		new(0x55, 0, (_) => new STIND_IX(8)),
	];

	public STIND_IX(int bytes) {
		Bytes = bytes;
	}

	public int Bytes { get; set; }

	public OpCode OpCode {
		get {
			return Bytes switch {
				1 => OpCodes.Stind_I1,
				2 => OpCodes.Stind_I2,
				4 => OpCodes.Stind_I4,
				8 => OpCodes.Stind_I8,
				_ => throw new NotSupportedException("No valid opcode for value " + Bytes)
			};
		}
	}

	public byte[] GetBytes() {
		return [Bytes switch {
			1 => 0x52,
			2 => 0x53,
			4 => 0x54,
			8 => 0x55,
			_ => throw new NotSupportedException("No valid opcode for value " + Bytes)
		}];
	}

    public string GetCIL(CILAssemblyFactory factory, ICILMethod method) {
		return $"stind.i{Bytes}";
    }

	public void ModifyStack(CILAssemblyFactory factory, ICILMethod method, Stack<ISignatureType> current) {
		current.Pop();
		current.Pop();
	}
    public bool AlwaysBranches => false;
	public bool SometimesBranches => false;
	public int? BranchTarget => null;
}
