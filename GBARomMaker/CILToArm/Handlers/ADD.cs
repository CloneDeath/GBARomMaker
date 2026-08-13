using System;
using System.Linq;
using System.Reflection.Emit;
using System.Reflection.Metadata;
using GBARomMaker.CILToArm.ControlFlow;

namespace GBARomMaker.CILToArm.Handlers;

public class ADD : ICILToArmHandler {
	public OpCode[] Handles => [OpCodes.Add];

	public ArmCode Handle(InstructionMetadata instruction) {
		var relevantStack = instruction.StackTypes?.Take(2).ToList() ?? throw new InvalidOperationException($"Stack not deep enough for an add! {instruction}");
		var stackTypeA = relevantStack[0].Code;
		var stackTypeB = relevantStack[1].Code;
	
		var stackTypeAIsInt32Compatible = stackTypeA == SignatureTypeCode.Int32
			|| stackTypeA == SignatureTypeCode.Pointer
			|| stackTypeA == SignatureTypeCode.Byte;

		var stackTypeBIsInt32Compatible = stackTypeB == SignatureTypeCode.Int32
			|| stackTypeB == SignatureTypeCode.Pointer
			|| stackTypeB == SignatureTypeCode.Byte;

		// see Table III.2: Binary Numeric Operations
		if (stackTypeAIsInt32Compatible && stackTypeBIsInt32Compatible) {
			return new ArmCode([
				$"pop sp!, {{ r1, r2 }} @ <{stackTypeA}, {stackTypeB}>",
				"add r0,r1,r2",
				"push sp!, { r0 }"
			]);
		} else if (stackTypeAIsInt32Compatible && stackTypeB == SignatureTypeCode.Single) {
			return new ArmCode([
				$"pop sp!, {{ r0, r1 }} @ <{stackTypeA}, {stackTypeB}>",
				"push sp!, { r1 }",
				"bl gba_int_to_float",
				"pop sp!, { r1 }",
				"bl gba_float_add",
				"push sp!, { r0 }"
			]) {
				IncludeFloat = true
			};
		} else if (stackTypeA == SignatureTypeCode.Single && stackTypeB == SignatureTypeCode.Single) {
			return new ArmCode([
				$"pop sp!, {{ r0, r1 }} @ <{stackTypeA}, {stackTypeB}>",
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
