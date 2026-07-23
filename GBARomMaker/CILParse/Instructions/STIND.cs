using System.Reflection.Emit;
using GBARomMaker.CIL;

namespace GBARomMaker.CILParse.Instructions;

public class STIND : CILInstruction {
	public static CILInstructionDefinition Definition = new(0x53, 0, (_) => new STIND());

	public OpCode OpCode => OpCodes.Stind_I2;

    public byte[] GetBytes() {
		return [0x53];
    }

    public string GetCIL(CILFactory factory) {
		return "stind.i2";
    }
}
