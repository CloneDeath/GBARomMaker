using System.Reflection.Emit;
using GBARomMaker.CILToArm.ControlFlow;

namespace GBARomMaker.CILToArm.Handlers;

public class SHL : ICILToArmHandler {
	public OpCode[] Handles => [OpCodes.Shl];

	public ArmCode Handle(InstructionMetadata instruction) {
		return new ArmCode([
			"pop sp!, { r0, r1 } @ shiftAmount, value",
			"lsl r2, r1, r0",
			"push sp!, { r2 }"
		]);
	}
}
