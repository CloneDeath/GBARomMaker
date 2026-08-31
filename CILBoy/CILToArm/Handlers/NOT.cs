using System.Reflection.Emit;
using CILBoy.CILToArm.ControlFlow;

namespace CILBoy.CILToArm.Handlers;

public class NOT : ICILToArmHandler {
	public OpCode[] Handles => [OpCodes.Not];

	public ArmCode Handle(InstructionMetadata instruction) {
		return new ArmCode([
			"pop { r0 }",
			"mvn r0, r0",
			"push { r0 }"
		]);
	}
}
