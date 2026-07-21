namespace GBARomMaker.ARM.ALU;

public interface ALUOp2 {
	public bool IsImmediate { get; }
	public byte[] ToBytes();
}
