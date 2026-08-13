using System;
using System.Linq;
using System.Reflection.Emit;
using GBARomMaker.CILToArm.ControlFlow;

namespace GBARomMaker.CILToArm.Handlers;

public class ADD : ICILToArmHandler {
	public OpCode[] Handles => [OpCodes.Add];

	public ArmCode Handle(InstructionMetadata instruction) {
		var relevantStack = instruction.StackTypes?.Take(2).ToList() ?? throw new InvalidOperationException($"Stack not deep enough for an add! {instruction}");
		var stackTypeA = relevantStack[1];
		var stackTypeB = relevantStack[0];

		// see Table III.2: Binary Numeric Operations
		if (stackTypeA.IsInt32Compatible() && stackTypeB.IsInt32Compatible()) {
			return new ArmCode([
				$"pop sp!, {{ r1, r2 }} @ <{stackTypeB}, {stackTypeA}>",
				"add r0, r1, r2",
				"push sp!, { r0 }"
			]);
		} else if (stackTypeA.IsInt32Compatible() && stackTypeB.IsSingle()) {
			return new ArmCode([
				$"pop sp!, {{ r0, r1 }} @ <{stackTypeB}, {stackTypeA}>",
				"push sp!, { r1 }",
				"bl gba_int_to_float",
				"pop sp!, { r1 }",
				"bl gba_float_add",
				"push sp!, { r0 }"
			]) {
				IncludeFloat = true
			};
		} else if (stackTypeA.IsSingle() && stackTypeB.IsSingle()) {
			return new ArmCode([
				$"pop sp!, {{ r0, r1 }} @ <{stackTypeB}, {stackTypeA}>",
				"bl gba_float_add",
				"push sp!, { r0 }"
			]) {
				IncludeFloat = true
			};
		} else {
			throw new NotImplementedException($"CIL 'add' not supported for types {stackTypeA} + {stackTypeB}. {instruction}");
		}
	}
}
