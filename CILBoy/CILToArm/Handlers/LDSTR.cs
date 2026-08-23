using System.Collections.Generic;
using System.Reflection.Emit;
using System.Text;
using CILBoy.CIL;
using CILBoy.CILToArm.ControlFlow;

namespace CILBoy.CILToArm.Handlers;

public class LDSTR(CILAssemblyFactory factory) : ICILToArmHandler {
	public OpCode[] Handles => [OpCodes.Ldstr];

	public ArmCode Handle(InstructionMetadata instruction) {
		var ldstr = (CILBoy.CILParse.Instructions.LDSTR)instruction.Instruction;
		var str = ldstr.GetString(factory);

		var ascii = Encoding.ASCII.GetBytes(str);
		var code = new List<string> {
			$"ldr v1, ={ascii.Length} @ strlen \"{Encoding.ASCII.GetString(ascii)}\"",
			"mov r0, v1",
			"add r0, r0, #4 @ length of array",
			"bl gba_malloc",
			"push sp!, { r0 }",
			"str v1, [r0], #4",
		};

		for (var i = 0; i < ascii.Length; i += 1) {
			var c = ascii[i];
			var phrase = Encoding.ASCII.GetString([c]);
			code.Add($"ldr r1, =0x{c:X2} @ \"{phrase}\"");
			code.Add("strb r1, [r0], #1");
		}
		return new ArmCode(code.ToArray());
	}
}
