using System.Reflection.Emit;
using GBARomMaker.CIL;

namespace GBARomMaker.CILParse.Instructions;

public class DUP : CILInstruction {
	public static CILInstructionDefinition Definition = new(0x25, 0, (_) => new DUP());

	public OpCode OpCode => OpCodes.Dup;

    public byte[] GetBytes() {
		return [0x25];
    }

    public string GetCIL(CILFactory factory) {
		return "dup";
    }
}
