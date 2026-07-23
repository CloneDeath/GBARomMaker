using System.Reflection.Emit;
using GBARomMaker.CIL;

namespace GBARomMaker.CILParse.Instructions;

public class AND : CILInstruction {
	public static CILInstructionDefinition Definition = new(0x5F, 0, (_) => new AND());

	public OpCode OpCode => OpCodes.And;

    public byte[] GetBytes() {
		return [0x5F];
    }

    public string GetCIL(CILFactory factory) {
		return "and";
    }
}
