using System;
using System.Linq;
using System.Reflection.Emit;
using CILBoy.CILToArm.ControlFlow;

namespace CILBoy.CILToArm.Handlers;

public class CEQ : ICILToArmHandler {
	public OpCode[] Handles => [OpCodes.Ceq];

	public ArmCode Handle(InstructionMetadata instruction) {
		var relevantStack = instruction.StackTypes?.Take(2).ToList() ?? throw new InvalidOperationException($"Stack not deep enough for a ceq! {instruction}");
		var stackTypeA = relevantStack[1];
		var stackTypeB = relevantStack[0];
	
		if (stackTypeA.IsInt32Compatible() && stackTypeB.IsInt32Compatible()) {
			return new ArmCode([
				"pop { r0, r1 }",
				"cmp r0, r1",
				"moveq r0, #1",
				"movne r0, #0",
				"push { r0 }"
			]);
		} else {
			throw new NotImplementedException($"CIL 'ceq' not supported for types {stackTypeA} == {stackTypeB}. {instruction}");
		}
	}
}
