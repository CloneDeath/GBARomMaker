using System.Collections.Generic;
using System.Reflection.Emit;
using System.Reflection.Metadata;
using CILBoy.CIL;

namespace CILBoy.CILParse.FEInstructions;

public class CGT_UN : CILInstruction {
	public static CILInstructionDefinition Definition = new(0x03, 0, (_) => new CGT_UN());

	public OpCode OpCode => OpCodes.Cgt_Un;

    public byte[] GetBytes() {
		return [0xFE, 0x03];
    }

    public string GetCIL(CILMethodDefinition method) {
		return "cgt.un";
    }

	public void ModifyStack(CILMethodDefinition method, Stack<ISignatureType> current) {
		current.Pop();
		current.Pop();
		current.Push(new SignatureType(SignatureTypeCode.Int32));
	}
    public bool AlwaysBranches => false;
	public bool SometimesBranches => false;
	public int? BranchTarget => null;
}
