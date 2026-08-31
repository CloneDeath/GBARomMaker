using CILBoy.CIL;
using CILBoy.CILToArm.ControlFlow;

namespace CILBoy.CILToArm.CallHandlers;

public class SystemEnumHasFlag : ICallHandler {
	public string Handles => "System.Enum.HasFlag";

	public ArmCode Handle(InstructionMetadata instruction, ICILMethod method) {
		return new ArmCode([
			"pop { r0, r1 } @ value, this",
			"ldr r0, [r0]",
			"ldr r1, [r1]",
			$"@ call {method}",
			"and r2, r0, r1",
			"cmp r2, r0",
			"ldreq r0, =1",
			"ldrne r0, =0",
			"push { r0 }"
		]);
	}
}
