using System.Reflection.Emit;
using CILBoy.CILToArm.ControlFlow;

namespace CILBoy.CILToArm.Handlers;

public class POP : ICILToArmHandler {
	public OpCode[] Handles => [OpCodes.Pop];

	public ArmCode Handle(InstructionMetadata instruction) {
		return new ArmCode("add sp, sp, #4");
	}
}
