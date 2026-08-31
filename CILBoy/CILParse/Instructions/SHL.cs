using System.Collections.Generic;
using System.Reflection.Emit;
using System.Reflection.Metadata;
using CILBoy.CIL;

namespace CILBoy.CILParse.Instructions;

public class SHL : CILInstruction {
	public static CILInstructionDefinition Definition = new(0x62, 0, (_) => new SHL());

	public OpCode OpCode => OpCodes.Shl;

    public byte[] GetBytes() {
		return [0x62];
    }

    public string GetCIL(CILMethodDefinition method) {
		return "shl";
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
