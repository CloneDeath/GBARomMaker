namespace GBARomMaker.ARM.Memory;

public interface IOffset {
	public abstract bool IsImmediate { get; }
	public abstract byte[] ToBytes();
}
