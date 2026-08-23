using CILBoy.CIL;
using CILBoy.CILToArm.ControlFlow;

namespace CILBoy.CILToArm.CallHandlers;

public class SystemObjectCtor : ICallHandler {
	public string Handles => "System.Object..ctor";

	public ArmCode Handle(InstructionMetadata instruction, ICILMethod method) {
		return new ArmCode($"add sp, sp, #4 @ Pop `this`; Calling '{method}'");
	}
}
