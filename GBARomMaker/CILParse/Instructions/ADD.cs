using System.Reflection.Emit;

namespace GBARomMaker.CILParse.Instructions;

public class ADD : CILInstruction {
	public static CILInstructionDefinition Definition = new(0x58, 0, (_) => new ADD());

	public override OpCode OpCode => OpCodes.Add;

    public override byte[] GetBytes() {
		return [0x58];
    }

    public override string GetCIL() {
		return "add";
    }
}
