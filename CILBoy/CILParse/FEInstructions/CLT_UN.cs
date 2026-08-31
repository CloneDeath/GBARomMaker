using System.Collections.Generic;
using System.Reflection.Emit;
using System.Reflection.Metadata;
using CILBoy.CIL;

namespace CILBoy.CILParse.FEInstructions;

public class CLT_UN : CILInstruction {
	public static CILInstructionDefinition Definition = new(0x05, 0, (_) => new CLT_UN());

	public OpCode OpCode => OpCodes.Clt_Un;

    public byte[] GetBytes() {
		return [0xFE, 0x05];
    }

    public string GetCIL(CILMethodDefinition method) {
		return "clt.un";
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
