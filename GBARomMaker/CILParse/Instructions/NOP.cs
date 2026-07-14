using System.Reflection.Emit;

namespace GBARomMaker.CILParse.Instructions;

public class NOP : CILInstruction {
	public static CILInstructionDefinition Definition = new(0x00, 0, (_) => new NOP());

	public override OpCode OpCode => OpCodes.Nop;

    public override byte[] GetBytes() {
		return [0x00];
    }

    public override string GetCIL() {
		return "nop";
    }
}
