using System.Reflection.Emit;

namespace GBARomMaker.CILParse.FEInstructions;

public class CEQ : CILInstruction {
	public static CILInstructionDefinition Definition = new(0x01, 0, (_) => new CEQ());

	public override OpCode OpCode => OpCodes.Ceq;

    public override byte[] GetBytes() {
		return [0xFE, 0x01];
    }

    public override string GetCIL() {
		return "ceq";
    }
}
