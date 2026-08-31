using System.Reflection.Emit;
using CILBoy.CIL;
using CILBoy.CILToArm.ControlFlow;

namespace CILBoy.CILToArm.Handlers;

public class STARG_S(ICILMethod method) : ICILToArmHandler {
	public OpCode[] Handles => [
		OpCodes.Starg_S,
	];

	public ArmCode Handle(InstructionMetadata instruction) {
		var starg = (CILBoy.CILParse.Instructions.STARG_S)instruction.Instruction;
		var argCount = method.ParameterCount + (method.IsInstance ? 1 : 0);
		var wordsBack = (argCount - starg.Argument) - 1;
		return new ArmCode([
			"pop { r0 }",
			// this might be sacrilege to the stack...
			$"str r0, [fp, #{wordsBack * 4}] @ arg {starg.Argument}"
		]);
	}
}
