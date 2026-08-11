using System.Collections.Generic;
using System.Reflection.Emit;
using System.Text;
using GBARomMaker.CIL;
using GBARomMaker.CILToArm.ControlFlow;

namespace GBARomMaker.CILToArm.Handlers;

public class LDSTR(CILFactory factory) : ICILToArmHandler {
	public OpCode[] Handles => [OpCodes.Ldstr];

	public ArmCode Handle(InstructionMetadata instruction) {
		var ldstr = (GBARomMaker.CILParse.Instructions.LDSTR)instruction.Instruction;
		var str = ldstr.GetString(factory);

		var ascii = Encoding.ASCII.GetBytes(str);
		var allocSize = 1 + (ascii.Length / 4) + ((ascii.Length % 4) == 0 ? 0 : 1);
		var code = new List<string> {
			"push sp!, { r8 }",
			$"ldr r1, ={ascii.Length} @ str \"{Encoding.ASCII.GetString(ascii)}\"",
			"str r1, [r8], #4",
		};

		for (var i = 0; i < ascii.Length; i += 1) {
			var c = ascii[i];
			var phrase = Encoding.ASCII.GetString([c]);
			code.Add($"ldr r1, =0x{c:X2} @ \"{phrase}\"");
			code.Add("strb r1, [r8], #1");
		}
		return new ArmCode(code.ToArray());
	}
}
