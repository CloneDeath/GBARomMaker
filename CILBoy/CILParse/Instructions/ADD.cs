using System.Collections.Generic;
using System.Reflection.Emit;
using CILBoy.CIL;

namespace CILBoy.CILParse.Instructions;

public class ADD : CILInstruction {
	public static CILInstructionDefinition Definition = new(0x58, 0, (_) => new ADD());

	public OpCode OpCode => OpCodes.Add;

    public byte[] GetBytes() {
		return [0x58];
    }

    public string GetCIL(ICILMethod method) {
		return "add";
    }

	public void ModifyStack(ICILMethod method, Stack<ISignatureType> current) {
		var type = current.Pop();
		current.Pop();
		current.Push(type);
	}
    public bool AlwaysBranches => false;
	public bool SometimesBranches => false;
	public int? BranchTarget => null;
}
