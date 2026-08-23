using System.Reflection.Emit;
using CILBoy.CILToArm.ControlFlow;

namespace CILBoy.CILToArm.Handlers;

public class STELEM_IX : ICILToArmHandler {
	public OpCode[] Handles => [OpCodes.Stelem_I4];

	public ArmCode Handle(InstructionMetadata instruction) {
		return new ArmCode([
			"pop sp!, { r0, r1, r2 } @ value, index, array",
			"add r1, r1, #1 @ first word is array length",
			"str r0, [r2, r1, lsl #2]"
		]);
	}
}
