using System;
using System.Linq;
using System.Reflection.Emit;
using System.Reflection.Metadata;
using GBARomMaker.CILToArm.ControlFlow;

namespace GBARomMaker.CILToArm.Handlers;

public class SUB : ICILToArmHandler {
	public OpCode[] Handles => [OpCodes.Sub];

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

		// see Table III.2: Binary Numeric Operations
		if (stackTypeAIsInt32Compatible && stackTypeBIsInt32Compatible) {
			return new ArmCode([
				$"pop sp!, {{ r1, r2 }} @ <{stackTypeB}, {stackTypeA}>",
				"sub r0, r2, r1",
				"push sp!, { r0 }"
			]);
		} else if (stackTypeAIsInt32Compatible && stackTypeB == SignatureTypeCode.Single) {
			return new ArmCode([
				$"pop sp!, {{ v1, v2 }} @ <{stackTypeB}, {stackTypeA}>",
				"mov r0, v2",
				"bl gba_int_to_float",
				"mov r1, v1",
				"bl gba_float_sub",
				"push sp!, { r0 }"
			]) {
				IncludeFloat = true
			};
		} else if (stackTypeA == SignatureTypeCode.Single && stackTypeB == SignatureTypeCode.Single) {
			return new ArmCode([
				$"pop sp!, {{ r1, r2 }} @ <{stackTypeB}, {stackTypeA}>",
				"mov r0, r2",
				"bl gba_float_sub",
				"push sp!, { r0 }"
			]) {
				IncludeFloat = true
			};
		} else {
			throw new NotImplementedException($"CIL 'sub' not supported for types {stackTypeA} - {stackTypeB}. {instruction}");
		}
	}
}
