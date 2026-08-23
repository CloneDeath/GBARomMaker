namespace CILBoy.ARM;

public class Word : IInstruction {
	public required byte[] Value { get; init; }

    public byte[] ToBytes() {
		return Value;
    }
}
