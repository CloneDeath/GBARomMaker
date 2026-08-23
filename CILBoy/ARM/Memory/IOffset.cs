namespace CILBoy.ARM.Memory;

public interface IOffset {
	public abstract bool IsImmediate { get; }
	public abstract byte[] ToBytes();
}
