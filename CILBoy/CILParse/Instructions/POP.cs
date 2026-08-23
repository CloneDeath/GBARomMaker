using System.Collections.Generic;
using System.Reflection.Emit;
using CILBoy.CIL;

namespace CILBoy.CILParse.Instructions;

public class POP : CILInstruction {
	public static CILInstructionDefinition Definition = new(0x26, 0, (_) => new POP());

	public OpCode OpCode => OpCodes.Pop;

    public byte[] GetBytes() {
		return [0x26];
    }

    public string GetCIL(CILAssemblyFactory factory, ICILMethod method) {
		return "pop";
    }

    public void ModifyStack(CILAssemblyFactory factory, ICILMethod method, Stack<ISignatureType> current) {
		current.Pop();
	}

    public bool AlwaysBranches => false;
	public bool SometimesBranches => false;
	public int? BranchTarget => null;
}
