namespace CILBoy.Compilation;

public interface IOperationAssembler {
	public string Operation { get; }
	public void Assemble(string line, TokenQueue tokens, ARMMachineCode code);
}
