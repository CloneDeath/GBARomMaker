using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using GBARomMaker.CIL;
using GBARomMaker.CILToArm.ControlFlow;

namespace GBARomMaker.CILToArm.Handlers;

public class RET(ICILMethod method) : ICILToArmHandler {
	public OpCode[] Handles => [OpCodes.Ret];

	public ArmCode Handle(InstructionMetadata instruction) {
		var localCount = method.GetLocalVariableTypes().Count();

		var assembly = new List<string>();
		if (method.HasReturnValue) {
			assembly.Add("pop sp!, { ip } @ return value");

		}
		assembly.AddRange([
			$"sub sp, fp, #{localCount * 4}",
			"ldmdb sp, { v1-v5, fp, lr }",
			$"add sp, sp, #{localCount * 4}"
		]);
		// pop any method parameters
		if (method.ParameterCount > 0 || method.IsInstance) {
			var argsToPop = (method.IsInstance ? 1 : 0) + method.ParameterCount;
			assembly.Add($"add sp, sp, #{argsToPop * 4} @ this: { method.IsInstance }; param count: {method.ParameterCount}");
		}
		if (method.HasReturnValue) {
			assembly.Add("push sp!, { ip }");
		}
		assembly.Add("bx lr");
		return new ArmCode(assembly.ToArray());
	}
}
