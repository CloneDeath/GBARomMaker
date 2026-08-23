using System.Collections.Generic;
using System.Reflection.Emit;
using System.Reflection.Metadata;
using CILBoy.CIL;

namespace CILBoy.CILParse.FEInstructions;

public class CGT : CILInstruction {
	public static CILInstructionDefinition Definition = new(0x02, 0, (_) => new CGT());

	public OpCode OpCode => OpCodes.Cgt;

    public byte[] GetBytes() {
		return [0xFE, 0x02];
    }

    public string GetCIL(CILAssemblyFactory factory, ICILMethod method) {
		return "cgt";
    }

	public void ModifyStack(CILAssemblyFactory factory, ICILMethod method, Stack<ISignatureType> current) {
		current.Pop();
		current.Pop();
		current.Push(new SignatureType(SignatureTypeCode.Int32));
	}
    public bool AlwaysBranches => false;
	public bool SometimesBranches => false;
	public int? BranchTarget => null;
}
