using System;
using System.Linq;
using System.Reflection.Emit;
using GBARomMaker.CILToArm.ControlFlow;

namespace GBARomMaker.CILToArm.Handlers;

public class CONV_U1 : ICILToArmHandler {
	public OpCode[] Handles => [OpCodes.Conv_U1];

	public ArmCode Handle(InstructionMetadata instruction) {
		var topOfStackType = instruction.StackTypes?.FirstOrDefault() ?? throw new InvalidOperationException($"Stack not deep enough for a conv.u1! {instruction}");
		if (topOfStackType.IsInt32Compatible()) {
			return new ArmCode([
				"pop sp!, { r1 }",
				"ldr r2, =0xFF",
				"and r0, r1, r2",
				"push sp!, { r0 }"
			]);
		} else if (topOfStackType.IsSingle()) {
			return new ArmCode([
				"pop sp!, { r0 }",
				$"bl gba_float_to_int @ <{topOfStackType}> to int32",
				"ldr r1, =0xFF",
				"and r0, r0, r1",
				"push sp!, { r0 }"
			]) {
				IncludeFloat = true
			};
		} else {
			throw new NotImplementedException($"CIL 'conv.u1' not supported for type {topOfStackType}. {instruction}");
		}
	}
}
