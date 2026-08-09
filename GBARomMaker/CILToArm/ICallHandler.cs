using GBARomMaker.CILToArm.ControlFlow;

namespace GBARomMaker.CILToArm;

public interface ICallHandler {
	public string Handles { get; }
	public ArmCode Handle(InstructionMetadata instruction);
}
