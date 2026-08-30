using System.Collections.Generic;
using System.Reflection.Emit;
using CILBoy.CIL;

namespace CILBoy.CILParse;

public interface CILInstruction {
	public string GetCIL(ICILMethod method);
	public OpCode OpCode { get; }
	public byte[] GetBytes();
    public void ModifyStack(ICILMethod method, Stack<ISignatureType> current);
    public bool AlwaysBranches { get; }
	public bool SometimesBranches { get; }
	public int? BranchTarget { get; }
}
