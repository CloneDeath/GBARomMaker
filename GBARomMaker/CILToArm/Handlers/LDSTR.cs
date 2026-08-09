using System.Collections.Generic;
using System.Reflection.Emit;
using GBARomMaker.CIL;
using GBARomMaker.CILToArm.ControlFlow;

namespace GBARomMaker.CILToArm.Handlers;

public class LDSTR(CILFactory factory) : ICILToArmHandler {
	public OpCode[] Handles => [OpCodes.Ldstr];

	public ArmCode Handle(InstructionMetadata instruction) {
		var ldstr = (GBARomMaker.CILParse.Instructions.LDSTR)instruction.Instruction;
		var str = ldstr.GetString(factory);

		var allocSize = 1 + (str.Length / 2) + (str.Length % 2);
		var code = new List<string> {
			"push sp!, { r8 }",
			"mov r0, r8",
			$"add r8, r8, #{allocSize} @ <len> + \"{str}\"",
			$"ldr r1, ={str.Length}",
			"str r1, [r0], #4",
		};

		foreach (var c in str) {
			code.Add($"ldr r1, =0x{c:X4} @ {c}");
			code.Add("strh r1, [r0], #2");
		}

		var needsPadding = (str.Length % 2) != 0;
		if (needsPadding) {
			code.Add($"ldr r1, =0 @ 0 padding");
			code.Add("strh r1, [r0], #2");
		}
		return new ArmCode(code.ToArray());
	}
}
