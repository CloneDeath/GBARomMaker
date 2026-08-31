using System;
using System.Linq;
using System.Reflection.Emit;
using System.Reflection.Metadata;
using CILBoy.CILToArm.ControlFlow;

namespace CILBoy.CILToArm.Handlers;

public class BOX : ICILToArmHandler {
	public OpCode[] Handles => [OpCodes.Box];

	public ArmCode Handle(InstructionMetadata instruction) {
		var type = instruction.StackTypes?.First() ?? throw new Exception("Failed to get type from stack");
		if (type.Code == SignatureTypeCode.TypeHandle) {
			return new ArmCode("nop"); // already boxed
		}

		if (type.Code == SignatureTypeCode.Int32) {
			return new ArmCode([
				"ldr r0, =4",
				"bl gba_malloc",
				"pop { r1 }",
				"str r1, [r0]",
				"push { r0 }"
			]);
		}

		throw new Exception($"Not sure how to box type '{type}'");
	}
}
