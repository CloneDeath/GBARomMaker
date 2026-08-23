using System.Collections.Generic;
using System.Reflection.Emit;
using CILBoy.CIL;

namespace CILBoy.CILParse.Instructions;

public class NOT : CILInstruction {
	public static CILInstructionDefinition Definition = new(0x66, 0, (_) => new NOT());

	public OpCode OpCode => OpCodes.Not;

    public byte[] GetBytes() {
		return [0x66];
    }

    public string GetCIL(CILAssemblyFactory factory, ICILMethod method) {
		return "not";
    }
    
	public void ModifyStack(CILAssemblyFactory factory, ICILMethod method, Stack<ISignatureType> current) {
	}

    public bool AlwaysBranches => false;
	public bool SometimesBranches => false;
	public int? BranchTarget => null;
}
