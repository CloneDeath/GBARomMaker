namespace GBARomMaker.CILParse;

public class CILInstructionDefinition {
	public delegate CILInstruction InstructionFactory(byte[] args);

	public byte OpCode { get; }
	public int ArgumentCount { get; }
	public InstructionFactory Factory { get; }
	
	public CILInstructionDefinition(byte opcode, int argCount, InstructionFactory factory) {
		this.OpCode = opcode;
		this.ArgumentCount = argCount;
		this.Factory = factory;
	}
}
