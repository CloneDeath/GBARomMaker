using System.Reflection.Emit;
using GBARomMaker.CILToArm.ControlFlow;

namespace GBARomMaker.CILToArm.Handlers;

public class LDC_I4 : ICILToArmHandler {
	public OpCode[] Handles => [OpCodes.Ldc_I4];

	public ArmCode Handle(InstructionMetadata instruction) {
		var ldc = (GBARomMaker.CILParse.Instructions.LDC_I4)instruction.Instruction;
		return new ArmCode([
			$"ldr r0, =0x{ldc.Data:X8}",
			"push sp!, { r0 }"
		]);
	}
}
