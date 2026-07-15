using System.Reflection.Emit;

namespace GBARomMaker.CILParse.Instructions;

public class MUL : CILInstruction {
	public static CILInstructionDefinition Definition = new(0x5A, 0, (_) => new MUL());

	public override OpCode OpCode => OpCodes.Mul;

    public override byte[] GetBytes() {
		return [0x5A];
    }

    public override string GetCIL() {
		return "mul";
    }
}
