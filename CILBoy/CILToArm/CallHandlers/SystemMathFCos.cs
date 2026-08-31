using CILBoy.CIL;
using CILBoy.CILToArm.ControlFlow;

namespace CILBoy.CILToArm.CallHandlers;

public class SystemMathFCos : ICallHandler {
	public string Handles => "System.MathF.Cos";

	public ArmCode Handle(InstructionMetadata instruction, ICILMethod method) {
		return new ArmCode([
			"pop { r0 }",
			$"bl gba_float_cos @ { method }",
			"push { r0 }"
		]) {
			IncludeSin = true
		};
	}
}
