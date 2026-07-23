using System.Reflection.Emit;
using GBARomMaker.CIL;

namespace GBARomMaker.CILParse.FEInstructions;

public class CGT : CILInstruction {
	public static CILInstructionDefinition Definition = new(0x02, 0, (_) => new CGT());

	public OpCode OpCode => OpCodes.Cgt;

    public byte[] GetBytes() {
		return [0xFE, 0x02];
    }

    public string GetCIL(CILFactory factory) {
		return "cgt";
    }
}
