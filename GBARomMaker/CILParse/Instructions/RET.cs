using System.Collections.Generic;
using System.Reflection.Emit;
using GBARomMaker.CIL;

namespace GBARomMaker.CILParse.Instructions;

public class RET : CILInstruction {
	public static CILInstructionDefinition Definition = new(0x2A, 0, (_) => new RET());

	public OpCode OpCode => OpCodes.Ret;

    public byte[] GetBytes() {
		return [0x2A];
    }

    public string GetCIL(CILFactory factory, ICILMethod method) {
		return "ret";
    }

	public void ModifyStack(CILFactory factory, ICILMethod method, Stack<ISignatureType> current) {
	}
    public bool AlwaysBranches => false;
	public bool SometimesBranches => false;
	public int? BranchTarget => null;
}
