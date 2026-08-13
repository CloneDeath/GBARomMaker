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
			"pop sp!, { v1 }",
			"lsl r0, v1, #2",
			"add r0, r0, #4 @ array length",
			"bl gba_malloc",
			"str v1, [r0]",
			"push sp!, { r0 }",
		]);
	}
}
