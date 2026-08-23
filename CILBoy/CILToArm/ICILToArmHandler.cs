using System.Reflection.Emit;
using CILBoy.CILToArm.ControlFlow;

namespace CILBoy.CILToArm;

public interface ICILToArmHandler {
	public OpCode[] Handles { get; }
	public ArmCode Handle(InstructionMetadata instruction);
}
