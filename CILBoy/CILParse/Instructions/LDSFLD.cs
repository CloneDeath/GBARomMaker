using System;
using CILBoy.CIL;
using System.Linq;
using System.Reflection.Emit;
using System.Collections.Generic;

namespace CILBoy.CILParse.Instructions;

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

	public string GetCIL(CILMethodDefinition method) {
		var field = method.Factory.GetFieldDefinition(MetadataToken);
		return $"ldsfld {field.FullName}";
	}
    
	public void ModifyStack(CILMethodDefinition method, Stack<ISignatureType> current) {
		var field = method.Factory.GetFieldDefinition(MetadataToken);
		current.Push(new SignatureType(field.Type));
	}

    public bool AlwaysBranches => false;
	public bool SometimesBranches => false;
	public int? BranchTarget => null;
}
