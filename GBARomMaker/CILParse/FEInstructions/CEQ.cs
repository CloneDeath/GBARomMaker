using System.Reflection.Emit;
using GBARomMaker.CIL;

namespace GBARomMaker.CILParse.FEInstructions;

public class CEQ : CILInstruction {
	public static CILInstructionDefinition Definition = new(0x01, 0, (_) => new CEQ());

	public OpCode OpCode => OpCodes.Ceq;

    public byte[] GetBytes() {
		return [0xFE, 0x01];
    }

    public string GetCIL(CILFactory factory) {
		return "ceq";
    }
}
