namespace GBARomMaker.Rom.Operations.ALU;

public abstract class ALUOp2 {
	public abstract bool IsImmediate { get; }

	public abstract byte[] ToBytes();
}
