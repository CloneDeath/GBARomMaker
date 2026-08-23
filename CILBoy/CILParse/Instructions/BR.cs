using System;
using CILBoy.CIL;
using System.Linq;
using System.Reflection.Emit;
using System.Collections.Generic;

namespace CILBoy.CILParse.Instructions;

public class BR : CILInstruction {
	public static CILInstructionDefinition[] Definitions = [
		new(0x38, 4, (args) => new BR(BitConverter.ToInt32(args))),
		new(0x2B, 1, (args) => new BR_S((sbyte)args[0])),
	];

	public int Target { get; set; }

	public BR(int target) {
		Target = target;
	}

	public OpCode OpCode => OpCodes.Br;

    public byte[] GetBytes() {
		return new byte[]{0x38}.Concat(BitConverter.GetBytes(Target)).ToArray();
    }

    public string GetCIL(CILAssemblyFactory factory, ICILMethod method) {
		return "br " + Target;
    }

	public void ModifyStack(CILAssemblyFactory factory, ICILMethod method, Stack<ISignatureType> current) {
	}
    public bool AlwaysBranches => true;
	public bool SometimesBranches => false;
	public int? BranchTarget => Target;
}

public class BR_S : CILInstruction {
	public sbyte Target { get; set; }

	public BR_S(sbyte target) {
		Target = target;
	}

	public OpCode OpCode => OpCodes.Br_S;

    public byte[] GetBytes() {
		return new byte[]{ 0x2B, (byte)Target };
    }

    public string GetCIL(CILAssemblyFactory factory, ICILMethod method) {
		return "br.s " + Target;
    }

	public void ModifyStack(CILAssemblyFactory factory, ICILMethod method, Stack<ISignatureType> current) {
	}
    public bool AlwaysBranches => true;
	public bool SometimesBranches => false;
	public int? BranchTarget => Target;
}
