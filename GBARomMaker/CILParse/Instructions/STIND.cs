using System.Reflection.Emit;

namespace GBARomMaker.CILParse.Instructions;

public class STIND : CILInstruction {
	public static CILInstructionDefinition Definition = new(0x53, 0, (_) => new STIND());

	public override OpCode OpCode => OpCodes.Stind_I2;

    public override byte[] GetBytes() {
		return [0x53];
    }

    public override string GetCIL() {
		return "stind.i2";
    }
}
