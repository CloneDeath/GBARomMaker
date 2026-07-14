using System.Reflection.Emit;

namespace GBARomMaker.CILParse.Instructions;

public class CONV : CILInstruction {
	public static CILInstructionDefinition Definition = new(0xD3, 0, (_) => new CONV());

	public override OpCode OpCode => OpCodes.Conv_I;

    public override byte[] GetBytes() {
		return [0xD3];
    }

    public override string GetCIL() {
		return "conv.i";
    }
}
