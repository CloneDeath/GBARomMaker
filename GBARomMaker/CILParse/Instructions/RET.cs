using System.Reflection.Emit;

namespace GBARomMaker.CILParse.Instructions;

public class RET : CILInstruction {
	public static CILInstructionDefinition Definition = new(0x2A, 0, (_) => new RET());

	public override OpCode OpCode => OpCodes.Ret;

    public override byte[] GetBytes() {
		return [0x2A];
    }

    public override string GetCIL() {
		return "ret";
    }
}
