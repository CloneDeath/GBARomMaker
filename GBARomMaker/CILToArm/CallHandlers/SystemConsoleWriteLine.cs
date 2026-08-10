using System;
using System.Reflection.Metadata;
using GBARomMaker.CIL;
using GBARomMaker.CILToArm.ControlFlow;

namespace GBARomMaker.CILToArm.CallHandlers;

public class SystemConsoleWriteLine(CILFactory factory) : ICallHandler {
	public string Handles => "System.Console.WriteLine";

	public ArmCode Handle(InstructionMetadata instruction) {
		var call = (GBARomMaker.CILParse.Instructions.CALL)instruction.Instruction;
		var method = factory.GetMethodDefinition(call.MetadataToken);
		var arguments = method.GetArgumentTypes();

		var argsString = string.Join(", ", arguments);
		var methodCall = $"{method.ReturnType} {method.FullName}({argsString})";

		if (arguments.Length == 1 && arguments[0].Code == SignatureTypeCode.String) {
			return new ArmCode([
				"pop sp!, { r0 }",
				$"bl mgba_log @ {methodCall}",
			]) {
				IncludeMGBALog = true
			};
		}

		throw new NotImplementedException($"No handler for \"{methodCall}\"");
	}
}
