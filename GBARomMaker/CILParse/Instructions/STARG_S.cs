using System.Collections.Generic;
using System.Reflection.Emit;
using GBARomMaker.CIL;

namespace GBARomMaker.CILParse.Instructions;

public class STARG_S : CILInstruction {
	public static CILInstructionDefinition Definition = new(0x10, 1, (args) => new STARG_S(args[0]));

	public STARG_S(byte argument) {
		Argument = argument;
	}

	public byte Argument { get; }

	public OpCode OpCode => OpCodes.Starg_S;

    public byte[] GetBytes() {
		return [0x10, Argument];
    }

    public string GetCIL(CILFactory factory, ICILMethod method) {
		return $"starg.s {Argument}";
    }
    
	public void ModifyStack(CILFactory factory, ICILMethod method, Stack<ISignatureType> current) {
		current.Pop();
	}

    public bool AlwaysBranches => false;
	public bool SometimesBranches => false;
	public int? BranchTarget => null;
}
