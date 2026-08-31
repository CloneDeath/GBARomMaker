using System.Reflection.Emit;
using CILBoy.CILToArm.ControlFlow;

namespace CILBoy.CILToArm.Handlers;

public class STLOC_X : ICILToArmHandler {
	public OpCode[] Handles => [
		OpCodes.Stloc_0,
		OpCodes.Stloc_1,
		OpCodes.Stloc_2,
		OpCodes.Stloc_3,
		OpCodes.Stloc_S,
	];

	public ArmCode Handle(InstructionMetadata instruction) {
		var stloc = (CILBoy.CILParse.ILocationInstruction)instruction.Instruction;
		var location = stloc.Location;
		return new ArmCode([
			"pop { r0 }",
			$"str r0, [fp, #-{(location+1) * 4}] @ local { location }"
		]);
	}
}
