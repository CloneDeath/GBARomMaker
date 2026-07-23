using System.Reflection.Emit;
using GBARomMaker.CIL;

namespace GBARomMaker.CILParse.Instructions;

public class SHL : CILInstruction {
	public static CILInstructionDefinition Definition = new(0x62, 0, (_) => new SHL());

	public OpCode OpCode => OpCodes.Shl;

    public byte[] GetBytes() {
		return [0x62];
    }

    public string GetCIL(CILFactory factory) {
		return "shl";
    }
}
