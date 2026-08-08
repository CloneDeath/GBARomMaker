using System.Collections.Generic;
using System.Reflection.Emit;
using GBARomMaker.CIL;

namespace GBARomMaker.CILParse.Instructions;

public class MUL : CILInstruction {
	public static CILInstructionDefinition Definition = new(0x5A, 0, (_) => new MUL());

	public OpCode OpCode => OpCodes.Mul;

    public byte[] GetBytes() {
		return [0x5A];
    }

    public string GetCIL(CILFactory factory, ICILMethod method) {
		return "mul";
    }
    
	public void ModifyStack(CILFactory factory, ICILMethod method, Stack<ISignatureType> current) {
		current.Pop();
	}

    public bool AlwaysBranches => false;
	public bool SometimesBranches => false;
	public int? BranchTarget => null;
}
