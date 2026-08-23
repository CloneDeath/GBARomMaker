using CILBoy.CIL;
using CILBoy.CILToArm.ControlFlow;

namespace CILBoy.CILToArm;

public interface ICallHandler {
	public string Handles { get; }
	public ArmCode Handle(InstructionMetadata instruction, ICILMethod method);
}
