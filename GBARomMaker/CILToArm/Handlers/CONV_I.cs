using System;
using System.Linq;
using System.Reflection.Emit;
using System.Reflection.Metadata;
using GBARomMaker.CILToArm.ControlFlow;

namespace GBARomMaker.CILToArm.Handlers;

public class CONV_I : ICILToArmHandler {
	public OpCode[] Handles => [OpCodes.Conv_I];

	public ArmCode Handle(InstructionMetadata instruction) {
		var topOfStackType = instruction.StackTypes?.FirstOrDefault() ?? throw new InvalidOperationException($"Stack not deep enough for a conv.i! {instruction}");
		var stackTypeIsInt32Compatible = topOfStackType == SignatureTypeCode.Int32
			|| topOfStackType == SignatureTypeCode.Pointer
			|| topOfStackType == SignatureTypeCode.Byte;
		if (stackTypeIsInt32Compatible) {
			return new ArmCode($"nop @ <{topOfStackType}> is int32 compatible");
		} else if (topOfStackType == SignatureTypeCode.Single) {
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
