using System;
using System.Collections.Generic;
using System.Linq;
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
			"mov r0, r8",
			$"add r8, r8, #{allocSize * 4} @ <len> + \"{str}\"",
			$"ldr r1, ={ascii.Length}",
			"str r1, [r0], #4",
		};

		for (var i = 0; i <= ascii.Length - 4; i += 4) {
			var wordBytes = ascii[i .. (i+4)];
			var word = BitConverter.ToUInt32(wordBytes);
			var phrase = Encoding.ASCII.GetString(wordBytes);
			code.Add($"ldr r1, =0x{word:X8} @ \"{phrase}\"");
			code.Add("str r1, [r0], #4");
		}
		if (ascii.Length % 4 > 0) {
			var remainingBytes = ascii.Length % 4;
			var padBytes = 4 - (ascii.Length % 4);
			var tailNib = ascii[^remainingBytes..].Concat(new byte[padBytes]).ToArray();
			var finalWord = BitConverter.ToUInt32(tailNib);
			var phrase = Encoding.ASCII.GetString(ascii[^remainingBytes..]);
			code.Add($"ldr r1, =0x{finalWord:X8} @ \"{phrase}\"");
			code.Add("str r1, [r0], #4");

		}

		return new ArmCode(code.ToArray());
	}
}
