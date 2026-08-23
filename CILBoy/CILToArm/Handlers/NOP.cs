using System.Reflection.Emit;
using CILBoy.CILToArm.ControlFlow;

namespace CILBoy.CILToArm.Handlers;

public class NOP : ICILToArmHandler {
	public OpCode[] Handles => [OpCodes.Nop];

	public ArmCode Handle(InstructionMetadata instruction) {
		return new ArmCode("nop");
	}
}
