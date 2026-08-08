using System.Reflection.Emit;
using GBARomMaker.CILToArm.ControlFlow;

namespace GBARomMaker.CILToArm.Handlers;

public class STELEM_REF : ICILToArmHandler {
	public OpCode[] Handles => [OpCodes.Stelem_Ref];

	public ArmCode Handle(InstructionMetadata instruction) {
		return new ArmCode([
			"pop sp!, { r0, r1, r2 } @ value, index, array",
			"add r1, r1, #1 @ first word is array length",
			"str r2, [r0, r1, lsl #2]"
		]);
	}
}
