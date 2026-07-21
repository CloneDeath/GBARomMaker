using System;
using System.Reflection.Emit;

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

	public override OpCode OpCode => Argument switch {
		0 => OpCodes.Ldarg_0,
		1 => OpCodes.Ldarg_1,
		2 => OpCodes.Ldarg_2,
		3 => OpCodes.Ldarg_3,
		_ => throw new InvalidOperationException()
	};

    public override byte[] GetBytes() {
		return [Argument switch {
			0 => 0x02,
			1 => 0x03,
			2 => 0x04,
			3 => 0x05,
			_ => throw new InvalidOperationException()
		}];
    }

    public override string GetCIL() {
		return "ldarg." + Argument;
    }
}
