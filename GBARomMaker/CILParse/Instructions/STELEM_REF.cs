using System.Collections.Generic;
using System.Reflection.Emit;
using System.Reflection.Metadata;
using GBARomMaker.CIL;

namespace GBARomMaker.CILParse.Instructions;

public class STELEM_REF : CILInstruction {
	public static CILInstructionDefinition Definition = new(0xA2, 0, (_) => new STELEM_REF());

	public OpCode OpCode => OpCodes.Stelem_Ref;

    public byte[] GetBytes() {
		return [0xA2];
    }

    public string GetCIL(CILFactory factory, ICILMethod method) {
		return "stelem.ref";
    }
    
	public void ModifyStack(CILFactory factory, ICILMethod method, Stack<SignatureTypeCode> current) {
		current.Pop();
		current.Pop();
		current.Pop();
	}

    public bool AlwaysBranches => false;
	public bool SometimesBranches => false;
	public int? BranchTarget => null;
}
