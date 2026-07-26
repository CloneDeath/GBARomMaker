using System;
using GBARomMaker.CIL;
using System.Linq;
using System.Reflection.Emit;

namespace GBARomMaker.CILParse.Instructions;

public class BRFALSE : CILInstruction {
	public static CILInstructionDefinition[] Definitions = [
		new(0x39, 4, (args) => new BRFALSE(BitConverter.ToInt32(args))),
		new(0x2C, 1, (args) => new BRFALSE_S((sbyte)args[0])),
	];

	public int Target { get; set; }

	public BRFALSE(int target) {
		Target = target;
	}

	public OpCode OpCode => OpCodes.Brfalse;

    public byte[] GetBytes() {
		return new byte[]{0x39}.Concat(BitConverter.GetBytes(Target)).ToArray();
    }

    public string GetCIL(CILFactory factory) {
		return "brfalse " + Target;
    }
}

public class BRFALSE_S : CILInstruction {
	public sbyte Target { get; set; }

	public BRFALSE_S(sbyte target) {
		Target = target;
	}

	public OpCode OpCode => OpCodes.Brfalse_S;

    public byte[] GetBytes() {
		return new byte[]{ 0x2C, (byte)Target };
    }

    public string GetCIL(CILFactory factory) {
		return "brfalse.s " + Target;
    }
}
