using System.Collections.Generic;
using System.Reflection.Emit;
using System.Reflection.Metadata;
using CILBoy.CIL;

namespace CILBoy.CILParse.Instructions;

public class AND : CILInstruction {
	public static CILInstructionDefinition Definition = new(0x5F, 0, (_) => new AND());

	public OpCode OpCode => OpCodes.And;

    public byte[] GetBytes() {
		return [0x5F];
    }

    public string GetCIL(CILMethodDefinition method) {
		return "and";
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
