using System.Collections.Generic;
using System.Reflection.Emit;
using GBARomMaker.CIL;

namespace GBARomMaker.CILParse;

public interface CILInstruction {
	public string GetCIL(CILFactory factory, ICILMethod method);
	public OpCode OpCode { get; }
	public byte[] GetBytes();
    public void ModifyStack(CILFactory factory, ICILMethod method, Stack<ISignatureType> current);
    public bool AlwaysBranches { get; }
	public bool SometimesBranches { get; }
	public int? BranchTarget { get; }
}
