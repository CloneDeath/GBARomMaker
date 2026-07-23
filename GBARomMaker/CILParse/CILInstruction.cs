using System.Reflection.Emit;
using GBARomMaker.CIL;

namespace GBARomMaker.CILParse;

public interface CILInstruction {
	public string GetCIL(CILFactory factory);
	public OpCode OpCode { get; }
	public byte[] GetBytes();
}
