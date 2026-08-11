using GBARomMaker.CIL;
using GBARomMaker.CILToArm.ControlFlow;

namespace GBARomMaker.CILToArm.CallHandlers;

public class SystemMathFSin : ICallHandler {
	public string Handles => "System.MathF.Sin";

	public ArmCode Handle(InstructionMetadata instruction, ICILMethod method) {
		return new ArmCode([
			"pop sp!, { r0 }",
			$"bl gba_float_sin @ { method }",
			"push sp!, { r0 }"
		]) {
			IncludeSin = true
		};
	}
}
