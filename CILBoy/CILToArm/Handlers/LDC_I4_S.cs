using System.Reflection.Emit;
using CILBoy.CILToArm.ControlFlow;

namespace CILBoy.CILToArm.Handlers;

public class LDC_I4_S : ICILToArmHandler {
	public OpCode[] Handles => [OpCodes.Ldc_I4_S];

	public ArmCode Handle(InstructionMetadata instruction) {
		var ldc = (CILBoy.CILParse.Instructions.LDC_I4_S)instruction.Instruction;
		return new ArmCode([
			$"ldr r0, =0x{ldc.Data:X2}",
			"push { r0 }"
		]);
	}
}
