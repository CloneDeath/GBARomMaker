namespace GBARomMaker.ARM;

public interface ILabeledInstruction : IInstruction {
	public void SetOffset(int offset);
}
