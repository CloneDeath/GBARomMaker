using System.Collections.Generic;
using System.Reflection.Emit;
using System.Reflection.Metadata;
using CILBoy.CIL;

namespace CILBoy.CILParse.Instructions;

public class LDLOCA_S : CILInstruction {
	public static CILInstructionDefinition Definition = new(0x12, 1, (args) => new LDLOCA_S(args[0]));

	public LDLOCA_S(byte location) {
		Location = location;
	}

	public byte Location { get; }

	public OpCode OpCode => OpCodes.Ldloca_S;

    public byte[] GetBytes() {
		return [0x12, Location];
    }

    public string GetCIL(CILMethodDefinition method) {
		return $"ldloca.s {Location}";
    }
    
	public void ModifyStack(CILMethodDefinition method, Stack<ISignatureType> current) {
		current.Push(new SignatureType(SignatureTypeCode.Pointer));
	}

    public bool AlwaysBranches => false;
	public bool SometimesBranches => false;
	public int? BranchTarget => null;
}
