using System.Reflection.Emit;

namespace GBARomMaker.CILParse.Instructions;

public class RET : CILInstruction {
	public static CILInstructionDefinition Definition = new(0x2A, 0, (_) => new RET());

	public OpCode OpCode => OpCodes.Ret;

    public byte[] GetBytes() {
		return [0x2A];
    }

    public string GetCIL() {
		return "ret";
    }
}
