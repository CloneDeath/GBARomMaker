using System;
using System.Linq;
using System.Reflection.Emit;

namespace GBARomMaker.CILParse.Instructions;

public class BRTRUE : CILInstruction {
	public static CILInstructionDefinition[] Definitions = [
		new(0x3A, 4, (args) => new BRTRUE(BitConverter.ToInt32(args))),
		new(0x2D, 1, (args) => new BRTRUE_S((sbyte)args[0])),
	];

	public int Target { get; set; }

	public BRTRUE(int target) {
		Target = target;
	}

	public OpCode OpCode => OpCodes.Brtrue;

    public byte[] GetBytes() {
		return new byte[]{0x3A}.Concat(BitConverter.GetBytes(Target)).ToArray();
    }

    public string GetCIL() {
		return "brtrue " + Target;
    }
}

public class BRTRUE_S : CILInstruction {
	public sbyte Target { get; set; }

	public BRTRUE_S(sbyte target) {
		Target = target;
	}

	public OpCode OpCode => OpCodes.Brtrue_S;

    public byte[] GetBytes() {
		return new byte[]{ 0x2D, (byte)Target };
    }

    public string GetCIL() {
		return "brtrue.s " + Target;
    }
}
