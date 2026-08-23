using System.Reflection.Emit;
using CILBoy.CILToArm.ControlFlow;

namespace CILBoy.CILToArm.Handlers;

public class LDC_I4_X : ICILToArmHandler {
	public OpCode[] Handles => [
		OpCodes.Ldc_I4_M1,
		OpCodes.Ldc_I4_0,
		OpCodes.Ldc_I4_1,
		OpCodes.Ldc_I4_2,
		OpCodes.Ldc_I4_3,
		OpCodes.Ldc_I4_4,
		OpCodes.Ldc_I4_5,
		OpCodes.Ldc_I4_6,
		OpCodes.Ldc_I4_7,
		OpCodes.Ldc_I4_8,
	];

	public ArmCode Handle(InstructionMetadata instruction) {
		var ldc = (CILBoy.CILParse.Instructions.LDC_I4_X)instruction.Instruction;
		return new ArmCode([
			$"ldr r0, =0x{ldc.Data:X2}",
			"push sp!, { r0 }"
		]);
	}
}
