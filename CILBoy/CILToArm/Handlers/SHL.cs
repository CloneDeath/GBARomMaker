using System.Reflection.Emit;
using CILBoy.CILToArm.ControlFlow;

namespace CILBoy.CILToArm.Handlers;

public class SHL : ICILToArmHandler {
	public OpCode[] Handles => [OpCodes.Shl];

	public ArmCode Handle(InstructionMetadata instruction) {
		return new ArmCode([
			"pop { r0, r1 } @ shiftAmount, value",
			"lsl r2, r1, r0",
			"push { r2 }"
		]);
	}
}
