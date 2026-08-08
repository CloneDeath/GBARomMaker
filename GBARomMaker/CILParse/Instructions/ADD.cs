using System.Collections.Generic;
using System.Reflection.Emit;
using GBARomMaker.CIL;

namespace GBARomMaker.CILParse.Instructions;

public class ADD : CILInstruction {
	public static CILInstructionDefinition Definition = new(0x58, 0, (_) => new ADD());

	public OpCode OpCode => OpCodes.Add;

    public byte[] GetBytes() {
		return [0x58];
    }

    public string GetCIL(CILFactory factory, ICILMethod method) {
		return "add";
    }

	public void ModifyStack(CILFactory factory, ICILMethod method, Stack<ISignatureType> current) {
		var type = current.Pop();
		current.Pop();
		current.Push(type);
	}
    public bool AlwaysBranches => false;
	public bool SometimesBranches => false;
	public int? BranchTarget => null;
}
