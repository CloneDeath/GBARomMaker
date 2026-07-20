using System.Reflection.Emit;

namespace GBARomMaker.CILParse.FEInstructions;

public class CGT : CILInstruction {
	public static CILInstructionDefinition Definition = new(0x02, 0, (_) => new CGT());

	public override OpCode OpCode => OpCodes.Cgt;

    public override byte[] GetBytes() {
		return [0xFE, 0x02];
    }

    public override string GetCIL() {
		return "cgt";
    }
}
