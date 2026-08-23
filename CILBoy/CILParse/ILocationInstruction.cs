namespace CILBoy.CILParse;

public interface ILocationInstruction : CILInstruction {
	public uint Location { get; }
}
