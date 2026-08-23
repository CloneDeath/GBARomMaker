using System;
using System.Linq;
using System.Reflection.Emit;
using CILBoy.CILToArm.ControlFlow;

namespace CILBoy.CILToArm.Handlers;

public class CLT : ICILToArmHandler {
	// TODO properly implement their differences...
	public OpCode[] Handles => [OpCodes.Clt, OpCodes.Clt_Un];

	public ArmCode Handle(InstructionMetadata instruction) {
		var relevantStack = instruction.StackTypes?.Take(2).ToList() ?? throw new InvalidOperationException($"Stack not deep enough for a clt! {instruction}");
		var stackTypeA = relevantStack[1];
		var stackTypeB = relevantStack[0];

		if (stackTypeA.IsInt32Compatible() && stackTypeB.IsInt32Compatible()) {
			return new ArmCode([
				"pop sp!, { r0, r1 }",
				"cmp r1, r0",
				"movlt r0, #1",
				"movge r0, #0",
				"push sp!, { r0 }"
			]);
		} else if (stackTypeA.IsSingle() && stackTypeB.IsSingle()) {
			return new ArmCode([
				$"pop sp!, {{ r1, r2 }} @ <{stackTypeB}, {stackTypeA}>",
				"mov r0, r2",
				"bl gba_float_sub",
				"lsr r0, r0, #31",
				"push sp!, { r0 }"
			]){
				IncludeFloat = true
			};
		} else {
			throw new NotImplementedException($"CIL 'clt' not supported for types {stackTypeA} < {stackTypeB}. {instruction}");
		}
	}
}
