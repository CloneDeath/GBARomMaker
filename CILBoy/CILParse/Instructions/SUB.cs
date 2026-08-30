using System.Collections.Generic;
using System.Reflection.Emit;
using CILBoy.CIL;

namespace CILBoy.CILParse.Instructions;

public class SUB : CILInstruction {
	public static CILInstructionDefinition Definition = new(0x59, 0, (_) => new SUB());

	public OpCode OpCode => OpCodes.Sub;

    public byte[] GetBytes() {
		return [0x59];
    }

    public string GetCIL(ICILMethod method) {
		return "sub";
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
