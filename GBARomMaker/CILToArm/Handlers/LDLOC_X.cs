using System.Reflection.Emit;
using GBARomMaker.CILToArm.ControlFlow;

namespace GBARomMaker.CILToArm.Handlers;

public class LDLOC_X : ICILToArmHandler {
	public OpCode[] Handles => [
		OpCodes.Ldloc_0,
		OpCodes.Ldloc_1,
		OpCodes.Ldloc_2,
		OpCodes.Ldloc_3,
		OpCodes.Ldloc_S,
	];

	public ArmCode Handle(InstructionMetadata instruction) {
		var ldloc = (GBARomMaker.CILParse.Instructions.ILDLOC)instruction.Instruction;
		var location = ldloc.Location;
		return new ArmCode([
			$"ldr r0, [fp, #-{(location+1) * 4}] @ local { location }",
			"push sp!, { r0 }"
		]);
	}
}
