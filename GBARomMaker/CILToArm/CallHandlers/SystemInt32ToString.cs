using GBARomMaker.CILToArm.ControlFlow;

namespace GBARomMaker.CILToArm.CallHandlers;

public class SystemInt32ToString : ICallHandler {
	public string Handles => "System.MathF.Cos";

	public ArmCode Handle(InstructionMetadata instruction) {
		return new ArmCode([
				TODO IMPLEMENT
			"pop sp!, { r0 }",
			"bl gba_float_cos",
			"push sp!, { r0 }"
		]) {
			IncludeString = true
		};
	}
}
