using System.Collections.Generic;
using System.Reflection.Emit;
using GBARomMaker.CIL;

namespace GBARomMaker.CILParse.Instructions;

public class NOP : CILInstruction {
	public static CILInstructionDefinition Definition = new(0x00, 0, (_) => new NOP());

	public OpCode OpCode => OpCodes.Nop;

    public byte[] GetBytes() {
		return [0x00];
    }

    public string GetCIL(CILFactory factory, ICILMethod method) {
		return "nop";
    }
    
	public void ModifyStack(CILFactory factory, ICILMethod method, Stack<ISignatureType> current) {
	}

    public bool AlwaysBranches => false;
	public bool SometimesBranches => false;
	public int? BranchTarget => null;
}
