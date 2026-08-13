using GBARomMaker.CIL;
using GBARomMaker.CILToArm.ControlFlow;

namespace GBARomMaker.CILToArm.CallHandlers;

public class SystemMathFFloor : ICallHandler {
	public string Handles => "System.MathF.Floor";

	public ArmCode Handle(InstructionMetadata instruction, ICILMethod method) {
		return new ArmCode([
			"pop sp!, { r0 }",
			$"@ call {method}",
			"bl gba_float_to_int",
			"bl gba_int_to_float",
			"push sp!, { r0 }"
		]);
	}
}
