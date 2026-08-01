using System.Collections.Generic;
using System.Reflection.Emit;
using System.Reflection.Metadata;
using GBARomMaker.CIL;

namespace GBARomMaker.CILParse;

public interface CILInstruction {
	public string GetCIL(CILFactory factory, ICILMethod method);
	public OpCode OpCode { get; }
	public byte[] GetBytes();
    public void ModifyStack(CILFactory factory, ICILMethod method, Stack<SignatureTypeCode> current);
    public bool AlwaysBranches { get; }
	public bool SometimesBranches { get; }
	public int? BranchTarget { get; }
}
