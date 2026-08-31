using System.Collections.Generic;
using System.Reflection.Emit;
using CILBoy.CIL;

namespace CILBoy.CILParse.Instructions;

public class DIV : CILInstruction {
	public static CILInstructionDefinition Definition = new(0x5B, 0, (_) => new DIV());

	public OpCode OpCode => OpCodes.Div;

    public byte[] GetBytes() {
		return [0x5B];
    }

    public string GetCIL(CILMethodDefinition method) {
		return "div";
    }

	public void ModifyStack(CILMethodDefinition method, Stack<ISignatureType> current) {
		var type = current.Pop();
		current.Pop();
		current.Push(type);
	}
    public bool AlwaysBranches => false;
	public bool SometimesBranches => false;
	public int? BranchTarget => null;
}
