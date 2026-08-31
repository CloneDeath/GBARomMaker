using System.Collections.Generic;
using System.Reflection.Emit;
using CILBoy.CIL;

namespace CILBoy.CILParse.Instructions;

public class NOP : CILInstruction {
	public static CILInstructionDefinition Definition = new(0x00, 0, (_) => new NOP());

	public OpCode OpCode => OpCodes.Nop;

    public byte[] GetBytes() {
		return [0x00];
    }

    public string GetCIL(CILMethodDefinition method) {
		return "nop";
    }
    
	public void ModifyStack(CILMethodDefinition method, Stack<ISignatureType> current) {
	}

    public bool AlwaysBranches => false;
	public bool SometimesBranches => false;
	public int? BranchTarget => null;
}
