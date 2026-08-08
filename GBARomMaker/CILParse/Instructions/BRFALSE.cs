using System;
using GBARomMaker.CIL;
using System.Linq;
using System.Reflection.Emit;
using System.Collections.Generic;

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

    public string GetCIL(CILFactory factory, ICILMethod method) {
		return "brfalse " + Target;
    }

	public void ModifyStack(CILFactory factory, ICILMethod method, Stack<ISignatureType> current) {
		current.Pop();
	}
    public bool AlwaysBranches => false;
	public bool SometimesBranches => true;
	public int? BranchTarget => Target;
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

    public string GetCIL(CILFactory factory, ICILMethod method) {
		return "brfalse.s " + Target;
    }
	
	public void ModifyStack(CILFactory factory, ICILMethod method, Stack<ISignatureType> current) {
		current.Pop();
	}
    public bool AlwaysBranches => false;
	public bool SometimesBranches => true;
	public int? BranchTarget => Target;
}
