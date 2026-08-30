using System.Collections.Generic;
using System.Reflection.Emit;
using System.Reflection.Metadata;
using CILBoy.CIL;

namespace CILBoy.CILParse.Instructions;

public class CONV : CILInstruction {
	public static CILInstructionDefinition Definition = new(0xD3, 0, (_) => new CONV());

	public OpCode OpCode => OpCodes.Conv_I;

    public byte[] GetBytes() {
		return [0xD3];
    }

    public string GetCIL(ICILMethod method) {
		return "conv.i";
    }

    public void ModifyStack(ICILMethod method, Stack<ISignatureType> current) {
		current.Pop();
		current.Push(new SignatureType(SignatureTypeCode.Int32));
	}

    public bool AlwaysBranches => false;
	public bool SometimesBranches => false;
	public int? BranchTarget => null;
}
