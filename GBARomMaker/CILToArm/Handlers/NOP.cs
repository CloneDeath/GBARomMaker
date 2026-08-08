using System.Reflection.Emit;
using GBARomMaker.CILToArm.ControlFlow;

namespace GBARomMaker.CILToArm.Handlers;

public class NOP : ICILToArmHandler {
	public OpCode[] Handles => [OpCodes.Nop];

	public string[] Handle(InstructionMetadata instruction) {
		return [
			"nop"
		];
	}
}
