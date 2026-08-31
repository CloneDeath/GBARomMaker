using System;
using System.Linq;
using System.Reflection.Emit;
using CILBoy.CILToArm.ControlFlow;

namespace CILBoy.CILToArm.Handlers;

public class ADD : ICILToArmHandler {
	public OpCode[] Handles => [OpCodes.Add];

	public ArmCode Handle(InstructionMetadata instruction) {
		var relevantStack = instruction.StackTypes?.Take(2).ToList() ?? throw new InvalidOperationException($"Stack not deep enough for an add! {instruction}");
		var stackTypeA = relevantStack[1];
		var stackTypeB = relevantStack[0];

		// see Table III.2: Binary Numeric Operations
		if (stackTypeA.IsInt32Compatible() && stackTypeB.IsInt32Compatible()) {
			return new ArmCode([
				$"pop {{ r1, r2 }} @ <{stackTypeB}, {stackTypeA}>",
				"add r0, r1, r2",
				"push { r0 }"
			]);
		} else if (stackTypeA.IsInt32Compatible() && stackTypeB.IsSingle()) {
			return new ArmCode([
				$"pop {{ r0, r1 }} @ <{stackTypeB}, {stackTypeA}>",
				"push { r1 }",
				"bl gba_int_to_float",
				"pop { r1 }",
				"bl gba_float_add",
				"push { r0 }"
			]) {
				IncludeFloat = true
			};
		} else if (stackTypeA.IsSingle() && stackTypeB.IsSingle()) {
			return new ArmCode([
				$"pop {{ r0, r1 }} @ <{stackTypeB}, {stackTypeA}>",
				"bl gba_float_add",
				"push { r0 }"
			]) {
				IncludeFloat = true
			};
		} else {
			throw new NotImplementedException($"CIL 'add' not supported for types {stackTypeA} + {stackTypeB}. {instruction}");
		}
	}
}
