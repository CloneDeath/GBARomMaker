using System.Reflection.Emit;

namespace GBARomMaker.CILParse.FEInstructions;

public class CLT : CILInstruction {
	public static CILInstructionDefinition Definition = new(0x04, 0, (_) => new CLT());

	public OpCode OpCode => OpCodes.Clt;

    public byte[] GetBytes() {
		return [0xFE, 0x04];
    }

    public string GetCIL() {
		return "clt";
    }
}
