using System;
using System.Reflection.Metadata;
using CILBoy.CIL;
using CILBoy.CILToArm.ControlFlow;

namespace CILBoy.CILToArm.CallHandlers;

public class SystemConsoleWriteLine : ICallHandler {
	public string Handles => "System.Console.WriteLine";

	public ArmCode Handle(InstructionMetadata instruction, ICILMethod method) {
		var arguments = method.GetArgumentTypes();
		if (arguments.Length == 1 && arguments[0].Code == SignatureTypeCode.String) {
			return new ArmCode([
				"pop { r0 }",
				$"bl mgba_log_string @ {method}",
			]) {
				IncludeMGBALog = true
			};
		}

		if (arguments.Length == 1 && arguments[0].Code == SignatureTypeCode.Int32) {
			return new ArmCode([
				"pop { r0 }",
				$"bl mgba_log_i4 @ {method}",
			]) {
				IncludeMGBALog = true
			};
		}

		throw new NotImplementedException($"No handler for \"{method}\"");
	}
}
