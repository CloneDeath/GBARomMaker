using System.Reflection.Emit;
using CILBoy.CILToArm.ControlFlow;

namespace CILBoy.CILToArm.Handlers;

public class LDC_R4 : ICILToArmHandler {
	public OpCode[] Handles => [OpCodes.Ldc_R4];

	public ArmCode Handle(InstructionMetadata instruction) {
		var ldcr = (CILBoy.CILParse.Instructions.LDC_R4)instruction.Instruction;
		return new ArmCode([
			$"ldr r0, =0x{ldcr.DataRaw:X8}",
			"push { r0 }"
		]);
	}
}
