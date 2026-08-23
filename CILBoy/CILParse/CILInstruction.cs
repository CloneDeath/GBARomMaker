using System.Collections.Generic;
using System.Reflection.Emit;
using CILBoy.CIL;

namespace CILBoy.CILParse;

public interface CILInstruction {
	public string GetCIL(CILAssemblyFactory factory, ICILMethod method);
	public OpCode OpCode { get; }
	public byte[] GetBytes();
    public void ModifyStack(CILAssemblyFactory factory, ICILMethod method, Stack<ISignatureType> current);
    public bool AlwaysBranches { get; }
	public bool SometimesBranches { get; }
	public int? BranchTarget { get; }
}
