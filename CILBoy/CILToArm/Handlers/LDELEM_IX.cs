using System.Reflection.Emit;
using CILBoy.CILToArm.ControlFlow;

namespace CILBoy.CILToArm.Handlers;

public class LDELEM_IX : ICILToArmHandler {
	public OpCode[] Handles => [OpCodes.Ldelem_I4];

	public ArmCode Handle(InstructionMetadata instruction) {
		return new ArmCode([
			"pop { r0, r1 } @ index, array",
			"add r0, r0, #1 @ skip length",
			"ldr r2, [r1, r0, lsl #2]",
			"push { r2 }"
		]);
	}
}
