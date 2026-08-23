using System.Reflection.Emit;
using CILBoy.CILToArm.ControlFlow;

namespace CILBoy.CILToArm.Handlers;

public class DUP : ICILToArmHandler {
	public OpCode[] Handles => [OpCodes.Dup];

	public ArmCode Handle(InstructionMetadata instruction) {
		return new ArmCode([
			"ldr r0, [sp]",
			"push sp!, { r0 }"
		]);
	}
}
