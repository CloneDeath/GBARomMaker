using System.Reflection.Emit;
using GBARomMaker.CILToArm.ControlFlow;

namespace GBARomMaker.CILToArm.Handlers;

public class NOP : ICILToArmHandler {
	public OpCode[] Handles => [OpCodes.Nop];

	public ArmCode Handle(InstructionMetadata instruction) {
		return new ArmCode("nop");
	}
}
