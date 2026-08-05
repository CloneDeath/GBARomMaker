using System.Collections.Generic;
using System.Reflection.Emit;
using System.Reflection.Metadata;
using GBARomMaker.CIL;

namespace GBARomMaker.CILParse.Instructions;

public class REM : CILInstruction {
	public static CILInstructionDefinition Definition = new(0x5D, 0, (_) => new REM());

	public OpCode OpCode => OpCodes.Rem;

    public byte[] GetBytes() {
		return [0x5D];
    }

    public string GetCIL(CILFactory factory, ICILMethod method) {
		return "rem";
    }

	public void ModifyStack(CILFactory factory, ICILMethod method, Stack<SignatureTypeCode> current) {
		var type = current.Pop();
		current.Pop();
		current.Push(type);
	}
    public bool AlwaysBranches => false;
	public bool SometimesBranches => false;
	public int? BranchTarget => null;
}
