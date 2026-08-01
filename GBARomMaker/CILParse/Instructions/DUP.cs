using System.Collections.Generic;
using System.Reflection.Emit;
using System.Reflection.Metadata;
using GBARomMaker.CIL;

namespace GBARomMaker.CILParse.Instructions;

public class DUP : CILInstruction {
	public static CILInstructionDefinition Definition = new(0x25, 0, (_) => new DUP());

	public OpCode OpCode => OpCodes.Dup;

    public byte[] GetBytes() {
		return [0x25];
    }

    public string GetCIL(CILFactory factory, ICILMethod method) {
		return "dup";
    }

    public void ModifyStack(CILFactory factory, ICILMethod method, Stack<SignatureTypeCode> current) {
		var type = current.Peek();
		current.Push(type);
	}

    public bool AlwaysBranches => false;
	public bool SometimesBranches => false;
	public int? BranchTarget => null;
}
