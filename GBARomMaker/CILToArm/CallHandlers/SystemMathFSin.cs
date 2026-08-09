using GBARomMaker.CILToArm.ControlFlow;

namespace GBARomMaker.CILToArm.CallHandlers;

public class SystemMathFSin : ICallHandler {
	public string Handles => "System.MathF.Sin";

	public ArmCode Handle(InstructionMetadata instruction) {
		return new ArmCode([
			"pop sp!, { r0 }",
			"bl gba_float_sin",
			"push sp!, { r0 }"
		]) {
			IncludeSin = true
		};
	}
}
