using System.Collections.Generic;
using System.Reflection.Emit;
using System.Reflection.Metadata;
using GBARomMaker.CIL;

namespace GBARomMaker.CILParse.FEInstructions;

public class CEQ : CILInstruction {
	public static CILInstructionDefinition Definition = new(0x01, 0, (_) => new CEQ());

	public OpCode OpCode => OpCodes.Ceq;

    public byte[] GetBytes() {
		return [0xFE, 0x01];
    }

    public string GetCIL(CILFactory factory, ICILMethod method) {
		return "ceq";
    }

	public void ModifyStack(CILFactory factory, ICILMethod method, Stack<ISignatureType> current) {
		current.Pop();
		current.Pop();
		current.Push(new SignatureType(SignatureTypeCode.Int32));
	}
    public bool AlwaysBranches => false;
	public bool SometimesBranches => false;
	public int? BranchTarget => null;
}
