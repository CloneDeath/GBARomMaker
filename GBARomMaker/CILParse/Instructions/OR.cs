using System.Reflection.Emit;
using GBARomMaker.CIL;

namespace GBARomMaker.CILParse.Instructions;

public class OR : CILInstruction {
	public static CILInstructionDefinition Definition = new(0x60, 0, (_) => new OR());

	public OpCode OpCode => OpCodes.Or;

    public byte[] GetBytes() {
		return [0x60];
    }

    public string GetCIL(CILFactory factory) {
		return "or";
    }
}
