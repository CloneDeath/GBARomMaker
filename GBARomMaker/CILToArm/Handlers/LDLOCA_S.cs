using System.Reflection.Emit;
using GBARomMaker.CILToArm.ControlFlow;

namespace GBARomMaker.CILToArm.Handlers;

public class LDLOCA_S : ICILToArmHandler {
	public OpCode[] Handles => [
		OpCodes.Ldloca_S,
	];

	public ArmCode Handle(InstructionMetadata instruction) {
		var ldloc = (GBARomMaker.CILParse.Instructions.LDLOCA_S)instruction.Instruction;
		var location = ldloc.Location;
		return new ArmCode([
			$"sub r0, fp, #{(location+1) * 4} @ local address { location }",
			"push sp!, { r0 }"
		]);
	}
}
