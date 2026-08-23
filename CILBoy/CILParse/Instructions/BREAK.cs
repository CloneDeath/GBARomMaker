using System.Collections.Generic;
using System.Reflection.Emit;
using CILBoy.CIL;

namespace CILBoy.CILParse.Instructions;

public class BREAK : CILInstruction {
	public static CILInstructionDefinition Definition = new(0x01, 0, (_) => new BREAK());

	public OpCode OpCode => OpCodes.Break;

    public byte[] GetBytes() {
		return [0x01];
    }

    public string GetCIL(CILAssemblyFactory factory, ICILMethod method) {
		return "break";
    }

	public void ModifyStack(CILAssemblyFactory factory, ICILMethod method, Stack<ISignatureType> current) {
	}
    public bool AlwaysBranches => false;
	public bool SometimesBranches => false;
	public int? BranchTarget => null;
}
