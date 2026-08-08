using System.Reflection.Emit;
using GBARomMaker.CILToArm.ControlFlow;

namespace GBARomMaker.CILToArm.Handlers;

public class LDLEN : ICILToArmHandler {
	public OpCode[] Handles => [OpCodes.Ldlen];

	public ArmCode Handle(InstructionMetadata instruction) {
		return new ArmCode([
			"pop sp!, { r0 }",
			"ldr r1, [r0, #0] @ Array.length",
			"push sp!, { r1 }"
		]);
	}
}
