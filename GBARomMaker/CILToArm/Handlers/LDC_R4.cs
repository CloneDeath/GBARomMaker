using System.Reflection.Emit;
using GBARomMaker.CILToArm.ControlFlow;

namespace GBARomMaker.CILToArm.Handlers;

public class LDC_R4 : ICILToArmHandler {
	public OpCode[] Handles => [OpCodes.Ldc_R4];

	public ArmCode Handle(InstructionMetadata instruction) {
		var ldcr = (GBARomMaker.CILParse.Instructions.LDC_R4)instruction.Instruction;
		return new ArmCode([
			$"ldr r0, =0x{ldcr.DataRaw:X8}",
			"push sp!, { r0 }"
		]);
	}
}
