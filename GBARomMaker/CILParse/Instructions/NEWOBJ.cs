using System;
using GBARomMaker.CIL;
using System.Linq;
using System.Reflection.Emit;
using System.Reflection.Metadata;
using System.Collections.Generic;

namespace GBARomMaker.CILParse.Instructions;

public class NEWOBJ : CILInstruction {
	public static CILInstructionDefinition Definition = new(0x73, 4, (args) => new NEWOBJ(args));

	public OpCode OpCode => OpCodes.Newobj;

	public NEWOBJ(byte[] args) {
		MetadataToken = BitConverter.ToInt32(args);
	}

	public int MetadataToken { get; set; }

	public byte[] GetBytes() {
		return new byte[]{0x73}.Concat(BitConverter.GetBytes(MetadataToken)).ToArray();
	}

	public string GetCIL(CILFactory factory, ICILMethod method) {
		var targetMethod = factory.GetMethodDefinition(MetadataToken);
		return "newobj " + targetMethod.FullName;
	}

    public void ModifyStack(CILFactory factory, ICILMethod method, Stack<SignatureTypeCode> current) {
		var targetMethod = factory.GetMethodDefinition(MetadataToken);
		var args = targetMethod.ParameterCount;
		for (var i = 0; i < args; i++) {
			current.Pop();
		}
		current.Push(SignatureTypeCode.Object);
	}

    public bool AlwaysBranches => false;
	public bool SometimesBranches => false;
	public int? BranchTarget => null;
}
