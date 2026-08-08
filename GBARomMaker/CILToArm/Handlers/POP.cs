using System.Reflection.Emit;
using GBARomMaker.CILToArm.ControlFlow;

namespace GBARomMaker.CILToArm.Handlers;

public class POP : ICILToArmHandler {
	public OpCode[] Handles => [OpCodes.Pop];

	public ArmCode Handle(InstructionMetadata instruction) {
		return new ArmCode("add sp, sp, #4");
	}
}
