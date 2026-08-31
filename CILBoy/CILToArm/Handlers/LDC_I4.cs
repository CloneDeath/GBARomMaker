using System.Reflection.Emit;
using CILBoy.CILToArm.ControlFlow;

namespace CILBoy.CILToArm.Handlers;

public class LDC_I4 : ICILToArmHandler {
	public OpCode[] Handles => [OpCodes.Ldc_I4];

	public ArmCode Handle(InstructionMetadata instruction) {
		var ldc = (CILBoy.CILParse.Instructions.LDC_I4)instruction.Instruction;
		return new ArmCode([
			$"ldr r0, =0x{ldc.Data:X8}",
			"push { r0 }"
		]);
	}
}
