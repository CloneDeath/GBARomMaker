using System.Collections.Generic;
using System.Reflection.Emit;
using System.Reflection.Metadata;
using CILBoy.CIL;

namespace CILBoy.CILParse.FEInstructions;

public class CLT : CILInstruction {
	public static CILInstructionDefinition Definition = new(0x04, 0, (_) => new CLT());

	public OpCode OpCode => OpCodes.Clt;

    public byte[] GetBytes() {
		return [0xFE, 0x04];
    }

    public string GetCIL(ICILMethod method) {
		return "clt";
    }

	public void ModifyStack(ICILMethod method, Stack<ISignatureType> current) {
		current.Pop();
		current.Pop();
		current.Push(new SignatureType(SignatureTypeCode.Int32));
	}
    public bool AlwaysBranches => false;
	public bool SometimesBranches => false;
	public int? BranchTarget => null;
}
