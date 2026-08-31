using CILBoy.CIL;
using CILBoy.CILToArm.ControlFlow;

namespace CILBoy.CILToArm.CallHandlers;

public class SystemMathFSin : ICallHandler {
	public string Handles => "System.MathF.Sin";

	public ArmCode Handle(InstructionMetadata instruction, ICILMethod method) {
		return new ArmCode([
			"pop { r0 }",
			$"bl gba_float_sin @ { method }",
			"push { r0 }"
		]) {
			IncludeSin = true
		};
	}
}
