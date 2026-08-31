using System.Reflection.Emit;
using CILBoy.CIL;
using CILBoy.CILToArm.ControlFlow;

namespace CILBoy.CILToArm.Handlers;

public class LDARG_X(ICILMethod method) : ICILToArmHandler {
	public OpCode[] Handles => [
		OpCodes.Ldarg_0,
		OpCodes.Ldarg_1,
		OpCodes.Ldarg_2,
		OpCodes.Ldarg_3,
	];

	public ArmCode Handle(InstructionMetadata instruction) {
		var ldarg = (CILBoy.CILParse.Instructions.LDARG)instruction.Instruction;
		var argCount = method.ParameterCount + (method.IsInstance ? 1 : 0);
		var wordsBack = (argCount - ldarg.Argument) - 1;
		return new ArmCode([
			$"ldr r0, [fp, #{wordsBack * 4}] @ arg {ldarg.Argument}",
			"push { r0 }"
		]);
	}
}
