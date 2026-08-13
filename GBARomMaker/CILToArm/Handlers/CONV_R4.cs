using System;
using System.Linq;
using System.Reflection.Emit;
using GBARomMaker.CILToArm.ControlFlow;

namespace GBARomMaker.CILToArm.Handlers;

public class CONV_R4 : ICILToArmHandler {
	public OpCode[] Handles => [OpCodes.Conv_R4];

	public ArmCode Handle(InstructionMetadata instruction) {
		var topOfStackType = instruction.StackTypes?.FirstOrDefault() ?? throw new InvalidOperationException($"Stack not deep enough for a conv.r4! {instruction}");
		if (topOfStackType.IsInt32Compatible()) {
			return new ArmCode([
				"pop sp!, { r0 }",
				$"bl gba_int_to_float @ <{topOfStackType}> to float",
				"push sp!, { r0 }"
			]);
		} else if (topOfStackType.IsSingle()) {
			return new ArmCode([
				$"nop @ <{topOfStackType}> to float",
			]) {
				IncludeFloat = true
			};
		} else {
			throw new NotImplementedException($"CIL 'conv.r4' not supported for type {topOfStackType}. {instruction}");
		}
	}
}
