using System;
using System.Linq;
using System.Reflection.Emit;

namespace GBARomMaker.CILParse.Instructions;

public class BR : CILInstruction {
	public static CILInstructionDefinition[] Definitions = [
		new(0x38, 4, (args) => new BR(BitConverter.ToInt32(args))),
		new(0x2B, 1, (args) => new BR_S((sbyte)args[0])),
	];

	public int Target { get; set; }

	public BR(int target) {
		Target = target;
	}

	public override OpCode OpCode => OpCodes.Br;

    public override byte[] GetBytes() {
		return new byte[]{0x38}.Concat(BitConverter.GetBytes(Target)).ToArray();
    }

    public override string GetCIL() {
		return "br " + Target;
    }
}

public class BR_S : CILInstruction {
	public sbyte Target { get; set; }

	public BR_S(sbyte target) {
		Target = target;
	}

	public override OpCode OpCode => OpCodes.Br_S;

    public override byte[] GetBytes() {
		return new byte[]{ 0x2B, (byte)Target };
    }

    public override string GetCIL() {
		return "br.s " + Target;
    }
}
