using System;
using System.Linq;
using System.Reflection.Emit;
using CILBoy.CILToArm.ControlFlow;

namespace CILBoy.CILToArm.Handlers;

public class CONV_I : ICILToArmHandler {
	public OpCode[] Handles => [OpCodes.Conv_I, OpCodes.Conv_I4];

	public ArmCode Handle(InstructionMetadata instruction) {
		var topOfStackType = instruction.StackTypes?.FirstOrDefault() ?? throw new InvalidOperationException($"Stack not deep enough for a conv.i! {instruction}");
		if (topOfStackType.IsInt32Compatible()) {
			return new ArmCode($"nop @ <{topOfStackType}> is int32 compatible");
		} else if (topOfStackType.IsSingle()) {
			return new ArmCode([
				"pop sp!, { r0 }",
				$"bl gba_float_to_int @ <{topOfStackType}> to int32",
				"push sp!, { r0 }"
			]) {
				IncludeFloat = true
			};
		} else {
			throw new NotImplementedException($"CIL 'conv.i' not supported for type {topOfStackType}. {instruction}");
		}

	}
}
