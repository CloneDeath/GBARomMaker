using System.Collections.Generic;
using System.Reflection.Emit;
using CILBoy.CIL;

namespace CILBoy.CILParse.Instructions;

public class DUP : CILInstruction {
	public static CILInstructionDefinition Definition = new(0x25, 0, (_) => new DUP());

	public OpCode OpCode => OpCodes.Dup;

    public byte[] GetBytes() {
		return [0x25];
    }

    public string GetCIL(CILAssemblyFactory factory, ICILMethod method) {
		return "dup";
    }

    public void ModifyStack(CILAssemblyFactory factory, ICILMethod method, Stack<ISignatureType> current) {
		var type = current.Peek();
		current.Push(type);
	}

    public bool AlwaysBranches => false;
	public bool SometimesBranches => false;
	public int? BranchTarget => null;
}
