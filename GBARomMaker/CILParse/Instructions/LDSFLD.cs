using System;
using GBARomMaker.CIL;
using System.Linq;
using System.Reflection.Emit;
using System.Reflection.Metadata;
using System.Collections.Generic;

namespace GBARomMaker.CILParse.Instructions;

public class LDSFLD : CILInstruction {
	public static CILInstructionDefinition Definition = new(0x7E, 4, (args) => new LDSFLD(args));

	public OpCode OpCode => OpCodes.Ldsfld;

	public LDSFLD(byte[] args) {
		MetadataToken = BitConverter.ToInt32(args);
	}

	public int MetadataToken { get; set; }

	public byte[] GetBytes() {
		return new byte[]{0x7E}.Concat(BitConverter.GetBytes(MetadataToken)).ToArray();
	}

	public string GetCIL(CILFactory factory, ICILMethod method) {
		var field = factory.GetFieldDefinition(MetadataToken);
		return $"ldsfld {field.FullName}";
	}
    
	public void ModifyStack(CILFactory factory, ICILMethod method, Stack<SignatureTypeCode> current) {
		throw new NotImplementedException();
	}

    public bool AlwaysBranches => false;
	public bool SometimesBranches => false;
	public int? BranchTarget => null;
}
