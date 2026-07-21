using System.Reflection.Emit;

namespace GBARomMaker.CILParse.Instructions;

public class CONV : CILInstruction {
	public static CILInstructionDefinition Definition = new(0xD3, 0, (_) => new CONV());

	public OpCode OpCode => OpCodes.Conv_I;

    public byte[] GetBytes() {
		return [0xD3];
    }

    public string GetCIL() {
		return "conv.i";
    }
}
