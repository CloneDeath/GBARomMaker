using System.Collections.Generic;
using System.Reflection.Emit;
using System.Reflection.Metadata;
using GBARomMaker.CIL;

namespace GBARomMaker.CILParse.Instructions;

public class CONV : CILInstruction {
	public static CILInstructionDefinition Definition = new(0xD3, 0, (_) => new CONV());

	public OpCode OpCode => OpCodes.Conv_I;

    public byte[] GetBytes() {
		return [0xD3];
    }

    public string GetCIL(CILFactory factory, ICILMethod method) {
		return "conv.i";
    }

    public void ModifyStack(CILFactory factory, ICILMethod method, Stack<SignatureTypeCode> current) {
		current.Pop();
		current.Push(SignatureTypeCode.Int32);
	}

    public bool AlwaysBranches => false;
	public bool SometimesBranches => false;
	public int? BranchTarget => null;
}
