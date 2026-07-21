using System.Reflection.Emit;

namespace GBARomMaker.CILParse.Instructions;

public class ADD : CILInstruction {
	public static CILInstructionDefinition Definition = new(0x58, 0, (_) => new ADD());

	public OpCode OpCode => OpCodes.Add;

    public byte[] GetBytes() {
		return [0x58];
    }

    public string GetCIL() {
		return "add";
    }
}
