using GBARomMaker.CIL;
using GBARomMaker.CILToArm.ControlFlow;

namespace GBARomMaker.CILToArm.CallHandlers;

public class SystemObjectCtor : ICallHandler {
	public string Handles => "System.Object..ctor";

	public ArmCode Handle(InstructionMetadata instruction, ICILMethod method) {
		return new ArmCode($"add sp, sp, #4 @ Pop `this`; Calling '{method}'");
	}
}
