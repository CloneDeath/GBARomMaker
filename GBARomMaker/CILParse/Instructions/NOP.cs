using System.Reflection.Emit;

namespace GBARomMaker.CILParse.Instructions;

public class NOP : CILInstruction {
	public static CILInstructionDefinition Definition = new(0x00, 0, (_) => new NOP());

	public OpCode OpCode => OpCodes.Nop;

    public byte[] GetBytes() {
		return [0x00];
    }

    public string GetCIL() {
		return "nop";
    }
}
