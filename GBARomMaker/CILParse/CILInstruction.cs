using System.Reflection.Emit;

namespace GBARomMaker.CILParse;

public abstract class CILInstruction {
	public abstract string GetCIL();
	public abstract OpCode OpCode { get; }
	public abstract byte[] GetBytes();
}
