using System;
using System.Linq;
using System.Reflection.Emit;
using System.Reflection.Metadata;
using GBARomMaker.CILToArm.ControlFlow;

namespace GBARomMaker.CILToArm.Handlers;

public class CONV_U2 : ICILToArmHandler {
	public OpCode[] Handles => [OpCodes.Conv_U2];

	public ArmCode Handle(InstructionMetadata instruction) {
		var topOfStackType = instruction.StackTypes?.FirstOrDefault()?.Code ?? throw new InvalidOperationException($"Stack not deep enough for a conv.u2! {instruction}");
		var stackTypeIsInt32Compatible = topOfStackType == SignatureTypeCode.Int32
			|| topOfStackType == SignatureTypeCode.Pointer
			|| topOfStackType == SignatureTypeCode.Byte;
		if (stackTypeIsInt32Compatible) {
			return new ArmCode([
				"pop sp!, { r1 }",
				"ldr r2, =0xFFFF",
				"and r0, r1, r2",
				"push sp!, { r0 }"
			]);
		} else if (topOfStackType == SignatureTypeCode.Single) {
			return new ArmCode([
				"pop sp!, { r0 }",
				$"bl gba_float_to_int @ <{topOfStackType}> to int32",
				"ldr r1, =0xFFFF",
				"and r0, r0, r1",
				"push sp!, { r0 }"
			]) {
				IncludeFloat = true
			};
		} else {
			throw new NotImplementedException($"CIL 'conv.u2' not supported for type {topOfStackType}. {instruction}");
		}
	}
}
