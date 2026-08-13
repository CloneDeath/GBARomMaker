using System;
using System.Linq;
using System.Reflection.Emit;
using System.Reflection.Metadata;
using GBARomMaker.CILToArm.ControlFlow;

namespace GBARomMaker.CILToArm.Handlers;

public class CLT : ICILToArmHandler {
	public OpCode[] Handles => [OpCodes.Clt];

	public ArmCode Handle(InstructionMetadata instruction) {
		var relevantStack = instruction.StackTypes?.Take(2).ToList() ?? throw new InvalidOperationException($"Stack not deep enough for a sub! {instruction}");
		// A - B
		var stackTypeA = relevantStack[1].Code;
		var stackTypeB = relevantStack[0].Code;
	
		var stackTypeAIsInt32Compatible = stackTypeA == SignatureTypeCode.Int32
			|| stackTypeA == SignatureTypeCode.Pointer
			|| stackTypeA == SignatureTypeCode.Byte;

		var stackTypeBIsInt32Compatible = stackTypeB == SignatureTypeCode.Int32
			|| stackTypeB == SignatureTypeCode.Pointer
			|| stackTypeB == SignatureTypeCode.Byte;

		if (stackTypeAIsInt32Compatible && stackTypeBIsInt32Compatible) {
			return new ArmCode([
				"pop sp!, { r0, r1 }",
				"cmp r1, r0",
				"movlt r0, #1",
				"movge r0, #0",
				"push sp!, { r0 }"
			]);
		} else {
			throw new NotImplementedException($"CIL 'clt' not supported for types {stackTypeA} < {stackTypeB}. {instruction}");
		}
	}
}
