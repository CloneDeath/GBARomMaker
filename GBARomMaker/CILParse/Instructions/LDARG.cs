using System;
using GBARomMaker.CIL;
using System.Reflection.Emit;
using System.Reflection.Metadata;
using System.Collections.Generic;

namespace GBARomMaker.CILParse.Instructions;

public class LDARG : CILInstruction {
	public static CILInstructionDefinition[] Definitions = [
		new(0x02, 0, (_) => new LDARG(0)),
		new(0x03, 0, (_) => new LDARG(1)),
		new(0x04, 0, (_) => new LDARG(2)),
		new(0x05, 0, (_) => new LDARG(3))
	];

	public byte Argument { get; set; }
	public LDARG(byte argument) {
		if (argument > 3) throw new InvalidOperationException();
		Argument = argument;
	}

	public OpCode OpCode => Argument switch {
		0 => OpCodes.Ldarg_0,
		1 => OpCodes.Ldarg_1,
		2 => OpCodes.Ldarg_2,
		3 => OpCodes.Ldarg_3,
		_ => throw new InvalidOperationException()
	};

    public byte[] GetBytes() {
		return [Argument switch {
			0 => 0x02,
			1 => 0x03,
			2 => 0x04,
			3 => 0x05,
			_ => throw new InvalidOperationException()
		}];
    }

    public string GetCIL(CILFactory factory, ICILMethod method) {
		return "ldarg." + Argument;
    }

    public void ModifyStack(CILFactory factory, ICILMethod method, Stack<SignatureTypeCode> current) {
		if (!method.IsInstance) {
			current.Push(method.GetArgumentTypes()[Argument]);
			return;
		}

		if (Argument == 0) {
			current.Push(SignatureTypeCode.Pointer);
			return;
		}

		current.Push(method.GetArgumentTypes()[Argument - 1]);
	}

    public bool AlwaysBranches => false;
	public bool SometimesBranches => false;
	public int? BranchTarget => null;
}
