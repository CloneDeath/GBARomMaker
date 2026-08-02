using System;
using GBARomMaker.CIL;
using System.Linq;
using System.Reflection.Emit;
using System.Reflection.Metadata;
using System.Collections.Generic;

namespace GBARomMaker.CILParse.Instructions;

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

	public string GetCIL(CILFactory factory, ICILMethod method) {
		var field = factory.GetFieldDefinition(MetadataToken);
		return $"ldfld {field.FullName}";
	}
    
	public void ModifyStack(CILFactory factory, ICILMethod method, Stack<SignatureTypeCode> current) {
		var field = factory.GetFieldDefinition(MetadataToken);
		current.Push(field.Type);
	}

    public bool AlwaysBranches => false;
	public bool SometimesBranches => false;
	public int? BranchTarget => null;
}
