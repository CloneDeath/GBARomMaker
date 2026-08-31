using System.Reflection.Emit;
using CILBoy.CILToArm.ControlFlow;

namespace CILBoy.CILToArm.Handlers;

public class LDLEN : ICILToArmHandler {
	public OpCode[] Handles => [OpCodes.Ldlen];

	public ArmCode Handle(InstructionMetadata instruction) {
		return new ArmCode([
			"pop { r0 }",
			"ldr r1, [r0, #0] @ Array.length",
			"push { r1 }"
		]);
	}
}
