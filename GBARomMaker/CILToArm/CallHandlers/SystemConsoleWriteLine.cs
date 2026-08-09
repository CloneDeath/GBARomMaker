using GBARomMaker.CILToArm.ControlFlow;

namespace GBARomMaker.CILToArm.CallHandlers;

public class SystemConsoleWriteLine : ICallHandler {
	public string Handles => "System.Console.WriteLine";

	public ArmCode Handle(InstructionMetadata instruction) {
		return new ArmCode([
			"pop sp!, { r0 }",
			"bl mgba_log",
		]) {
			IncludeMGBALog = true
		};
	}
}
