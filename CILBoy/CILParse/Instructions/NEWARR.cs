using System;
using CILBoy.CIL;
using System.Linq;
using System.Reflection.Emit;
using System.Reflection.Metadata;
using System.Collections.Generic;

namespace CILBoy.CILParse.Instructions;

public class NEWARR : CILInstruction {
	public static CILInstructionDefinition Definition = new(0x8D, 4, (args) => new NEWARR(args));

	public OpCode OpCode => OpCodes.Newarr;

	public NEWARR(byte[] args) {
		MetadataToken = BitConverter.ToInt32(args);
	}

	public int MetadataToken { get; set; }

	public byte[] GetBytes() {
		return new byte[]{0x8D}.Concat(BitConverter.GetBytes(MetadataToken)).ToArray();
	}

	public string GetCIL(CILMethodDefinition method) {
		var targetType = method.Factory.GetTypeDefinition(MetadataToken);
		return "newarr " + targetType.FullName;
	}

    public void ModifyStack(CILMethodDefinition method, Stack<ISignatureType> current) {
		current.Pop(); // numElems
		// todo, figure out how to get the inner type
		var innerType = new SignatureType(SignatureTypeCode.Object);
		current.Push(new ArraySignatureType(innerType));
	}

    public bool AlwaysBranches => false;
	public bool SometimesBranches => false;
	public int? BranchTarget => null;
}
