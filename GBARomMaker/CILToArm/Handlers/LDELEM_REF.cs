using System.Reflection.Emit;
using GBARomMaker.CILToArm.ControlFlow;

namespace GBARomMaker.CILToArm.Handlers;

public class LDELEM_REF : ICILToArmHandler {
	public OpCode[] Handles => [OpCodes.Ldelem_Ref];

	public ArmCode Handle(InstructionMetadata instruction) {
		return new ArmCode([
			"pop sp!, { r0, r1 } @ index, array",
			"add r0, r0, #1 @ skip length",
			"ldr r2, [r1, r0, lsl #2]",
			"push sp!, { r2 }"
		]);
	}
}
