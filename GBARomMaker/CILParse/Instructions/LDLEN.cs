using System.Collections.Generic;
using System.Reflection.Emit;
using System.Reflection.Metadata;
using GBARomMaker.CIL;

namespace GBARomMaker.CILParse.Instructions;

public class LDLEN : CILInstruction {
	public static CILInstructionDefinition Definition = new(0x8E, 0, (_) => new LDLEN());

	public OpCode OpCode => OpCodes.Ldlen;

    public byte[] GetBytes() {
		return [0x8E];
    }

    public string GetCIL(CILFactory factory, ICILMethod method) {
		return "ldlen";
    }
    
	public void ModifyStack(CILFactory factory, ICILMethod method, Stack<SignatureTypeCode> current) {
		current.Pop();
		current.Push(SignatureTypeCode.UInt32);
	}

    public bool AlwaysBranches => false;
	public bool SometimesBranches => false;
	public int? BranchTarget => null;
}
