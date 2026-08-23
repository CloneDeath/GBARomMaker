using System.Reflection.Emit;
using CILBoy.CILToArm.ControlFlow;

namespace CILBoy.CILToArm.Handlers;

public class LDIND_U2 : ICILToArmHandler {
	public OpCode[] Handles => [OpCodes.Ldind_U2];

	public ArmCode Handle(InstructionMetadata instruction) {
		return new ArmCode([
			"pop sp!, { r0 }",
			"ldrh r1, [r0]",
			"push sp!, { r1 }",
		]);
	}
}
