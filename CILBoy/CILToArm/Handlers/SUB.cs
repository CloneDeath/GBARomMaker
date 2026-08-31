using System;
using System.Linq;
using System.Reflection.Emit;
using CILBoy.CILToArm.ControlFlow;

namespace CILBoy.CILToArm.Handlers;

public class SUB : ICILToArmHandler {
	public OpCode[] Handles => [OpCodes.Sub];

	public ArmCode Handle(InstructionMetadata instruction) {
		var relevantStack = instruction.StackTypes?.Take(2).ToList() ?? throw new InvalidOperationException($"Stack not deep enough for a sub! {instruction}");
		var stackTypeA = relevantStack[1];
		var stackTypeB = relevantStack[0];

		// see Table III.2: Binary Numeric Operations
		if (stackTypeA.IsInt32Compatible() && stackTypeB.IsInt32Compatible()) {
			return new ArmCode([
				$"pop {{ r1, r2 }} @ <{stackTypeB}, {stackTypeA}>",
				"sub r0, r2, r1",
				"push { r0 }"
			]);
		} else if (stackTypeA.IsInt32Compatible() && stackTypeB.IsSingle()) {
			return new ArmCode([
				$"pop {{ v1, v2 }} @ <{stackTypeB}, {stackTypeA}>",
				"mov r0, v2",
				"bl gba_int_to_float",
				"mov r1, v1",
				"bl gba_float_sub",
				"push { r0 }"
			]) {
				IncludeFloat = true
			};
		} else if (stackTypeA.IsSingle() && stackTypeB.IsSingle()) {
			return new ArmCode([
				$"pop {{ r1, r2 }} @ <{stackTypeB}, {stackTypeA}>",
				"mov r0, r2",
				"bl gba_float_sub",
				"push { r0 }"
			]) {
				IncludeFloat = true
			};
		} else {
			throw new NotImplementedException($"CIL 'sub' not supported for types {stackTypeA} - {stackTypeB}. {instruction}");
		}
	}
}
