using CILBoy.CIL;
using CILBoy.CILToArm.ControlFlow;

namespace CILBoy.CILToArm.CallHandlers;

public class SystemInt32ToString : ICallHandler {
	public string Handles => "System.Int32.ToString";

	public ArmCode Handle(InstructionMetadata instruction, ICILMethod method) {
		return new ArmCode([
			"pop { r1 }",
			"ldr r0, [r1]",
			$"bl gba_i4_to_string @ { method }",
			"push { r0 }"
		]) {
			IncludeString = true
		};
	}
}
