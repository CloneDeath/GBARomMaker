using System.Reflection.Emit;

namespace GBARomMaker.CILParse.FEInstructions;

public class CLT : CILInstruction {
	public static CILInstructionDefinition Definition = new(0x04, 0, (_) => new CLT());

	public override OpCode OpCode => OpCodes.Clt;

    public override byte[] GetBytes() {
		return [0xFE, 0x04];
    }

    public override string GetCIL() {
		return "clt";
    }
}
