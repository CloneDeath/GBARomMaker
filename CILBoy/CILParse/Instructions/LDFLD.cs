using System;
using CILBoy.CIL;
using System.Linq;
using System.Reflection.Emit;
using System.Collections.Generic;

namespace CILBoy.CILParse.Instructions;

public class LDFLD : CILInstruction {
	public static CILInstructionDefinition Definition = new(0x7B, 4, (args) => new LDFLD(args));

	public OpCode OpCode => OpCodes.Ldfld;

	public LDFLD(byte[] args) {
		MetadataToken = BitConverter.ToInt32(args);
	}

	public int MetadataToken { get; set; }

	public byte[] GetBytes() {
		return new byte[]{0x7B}.Concat(BitConverter.GetBytes(MetadataToken)).ToArray();
	}

	public string GetCIL(CILAssemblyFactory factory, ICILMethod method) {
		var field = factory.GetFieldDefinition(MetadataToken);
		return $"ldfld {field.FullName}";
	}
    
	public void ModifyStack(CILAssemblyFactory factory, ICILMethod method, Stack<ISignatureType> current) {
		var field = factory.GetFieldDefinition(MetadataToken);
		current.Push(new SignatureType(field.Type));
	}

    public bool AlwaysBranches => false;
	public bool SometimesBranches => false;
	public int? BranchTarget => null;
}
