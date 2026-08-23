using System;
using System.Linq;
using System.Reflection.Emit;
using CILBoy.CILToArm.ControlFlow;

namespace CILBoy.CILToArm.Handlers;

public class AND : ICILToArmHandler {
	public OpCode[] Handles => [OpCodes.And];

	public ArmCode Handle(InstructionMetadata instruction) {
		var relevantStack = instruction.StackTypes?.Take(2).ToList() ?? throw new InvalidOperationException($"Stack not deep enough for an and! {instruction}");
		var stackTypeA = relevantStack[1];
		var stackTypeB = relevantStack[0];

		// see Table III.2: Binary Numeric Operations
		if (stackTypeA.IsInt32Compatible() && stackTypeB.IsInt32Compatible()) {
			return new ArmCode([
				$"pop sp!, {{ r1, r2 }} @ <{stackTypeB}, {stackTypeA}>",
				"and r0, r1, r2",
				"push sp!, { r0 }"
			]);
		} else {
			throw new NotImplementedException($"CIL 'and' not supported for types {stackTypeA} & {stackTypeB}. {instruction}");
		}
	}
}
