using GBARomMaker.CIL;
using GBARomMaker.CILToArm.ControlFlow;

namespace GBARomMaker.CILToArm.CallHandlers;

public class SystemStringConcat : ICallHandler {
	public string Handles => "System.String.Concat";

	public ArmCode Handle(InstructionMetadata instruction, ICILMethod method) {
		return new ArmCode([
			"pop sp!, { r0, r1 } @ right, left",
			"mov r2, r0 @ swap r0, r1",
			"mov r0, r1",
			"mov r1, r2",
			$"bl gba_string_concat @ { method }",
			"push sp!, { r0 }",
		]) {
			IncludeString = true
		};
	}
}
