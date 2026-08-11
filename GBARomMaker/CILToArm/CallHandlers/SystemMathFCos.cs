using GBARomMaker.CIL;
using GBARomMaker.CILToArm.ControlFlow;

namespace GBARomMaker.CILToArm.CallHandlers;

public class SystemMathFCos : ICallHandler {
	public string Handles => "System.MathF.Cos";

	public ArmCode Handle(InstructionMetadata instruction, ICILMethod method) {
		return new ArmCode([
			"pop sp!, { r0 }",
			$"bl gba_float_cos @ { method }",
			"push sp!, { r0 }"
		]) {
			IncludeSin = true
		};
	}
}
