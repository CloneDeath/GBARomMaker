using System;
using System.Reflection.Metadata;
using GBARomMaker.CIL;
using GBARomMaker.CILToArm.ControlFlow;

namespace GBARomMaker.CILToArm.CallHandlers;

public class SystemConvertToByte : ICallHandler {
	public string Handles => "System.Convert.ToByte";

	public ArmCode Handle(InstructionMetadata instruction, ICILMethod method) {
		var args = method.GetArgumentTypes();
		if (args.Length != 1) throw new NotImplementedException($"Only 1 args is implemented, {method}");
		if (args[0].Code != SignatureTypeCode.Single) throw new NotImplementedException($"Only floats supported. {method}");
		return new ArmCode([
			"pop sp!, { r0 }",
			$"@ call {method}",
			"bl gba_float_to_int",
			"and r0, r0, #0xFF",
			"push sp!, { r0 }"
		]);
	}
}
