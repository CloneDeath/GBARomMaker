using System.Collections.Generic;
using System.Reflection.Emit;
using System.Reflection.Metadata;
using GBARomMaker.CIL;

namespace GBARomMaker.CILParse.Instructions;

public class OR : CILInstruction {
	public static CILInstructionDefinition Definition = new(0x60, 0, (_) => new OR());

	public OpCode OpCode => OpCodes.Or;

    public byte[] GetBytes() {
		return [0x60];
    }

    public string GetCIL(CILFactory factory, ICILMethod method) {
		return "or";
    }

	public void ModifyStack(CILFactory factory, ICILMethod method, Stack<SignatureTypeCode> current) {
		current.Pop();
		current.Pop();
		current.Push(SignatureTypeCode.Int32);
	}
    public bool AlwaysBranches => false;
	public bool SometimesBranches => false;
	public int? BranchTarget => null;
}
