using GBARomMaker.CIL;
using GBARomMaker.CILToArm.ControlFlow;

namespace GBARomMaker.CILToArm.CallHandlers;

public class SystemObjectCtor(CILFactory factory) : ICallHandler {
	public string Handles => "System.Object..ctor";

	public ArmCode Handle(InstructionMetadata instruction) {
		var call = (GBARomMaker.CILParse.Instructions.CALL)instruction.Instruction;
		var method = factory.GetMethodDefinition(call.MetadataToken);
		return new ArmCode($"add sp, sp, #4 @ Pop `this`; Calling '{method.FullName}'");
	}
}
