using System.Reflection.Emit;
using GBARomMaker.CIL;
using GBARomMaker.CILToArm.ControlFlow;

namespace GBARomMaker.CILToArm.Handlers;

public class NEWARR(CILFactory factory) : ICILToArmHandler {
	public OpCode[] Handles => [OpCodes.Newarr];

	public ArmCode Handle(InstructionMetadata instruction) {
		var newarr = (GBARomMaker.CILParse.Instructions.NEWARR)instruction.Instruction;
		var typeDefinition = factory.GetTypeDefinition(newarr.MetadataToken);
		// TODO: Clear out the memory of the array...
		return new ArmCode([
			"pop sp!, { r0 }",
			$"push sp!, {{ r8 }} @ newarr {typeDefinition.FullName}",
			"ldr r1, =4",
			"mul r0, r0, r1",
			"add r0, r0, #4 @ +length of array",
			"add r8, r8, r0"
		]);
	}
}
