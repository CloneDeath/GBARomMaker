using System.Reflection.Emit;
using GBARomMaker.CILToArm.ControlFlow;

namespace GBARomMaker.CILToArm;

public interface ICILToArmHandler {
	public OpCode[] Handles { get; }
	public ArmCode Handle(InstructionMetadata instruction);
}
