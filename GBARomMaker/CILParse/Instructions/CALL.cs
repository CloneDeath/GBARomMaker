using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using System.Reflection.Metadata;
using GBARomMaker.CIL;

namespace GBARomMaker.CILParse.Instructions;

public class CALL : CILInstruction {
	public static CILInstructionDefinition Definition = new(0x28, 4, (args) => new CALL(args));

	public OpCode OpCode => OpCodes.Call;

	public CALL(byte[] args) {
		MetadataToken = BitConverter.ToInt32(args);
	}

	public int MetadataToken { get; set; }

	public byte[] GetBytes() {
		return new byte[]{0x28}.Concat(BitConverter.GetBytes(MetadataToken)).ToArray();
	}

	public string GetCIL(CILFactory factory, ICILMethod method) {
		var targetMethod = factory.GetMethodDefinition(MetadataToken);
		return "call " + targetMethod.FullName;
	}

    public void ModifyStack(CILFactory factory, ICILMethod method, Stack<SignatureTypeCode> current) {
		var targetMethod = factory.GetMethodDefinition(MetadataToken);
		var args = targetMethod.ParameterCount + (targetMethod.IsInstance ? 1 : 0);
		for (var i = 0; i < args; i++) {
			current.Pop();
		}
		if (targetMethod.HasReturnValue) {
			if (targetMethod.ReturnType != SignatureTypeCode.Int32) throw new NotImplementedException("Only return types of int supported");
			current.Push(SignatureTypeCode.Int32);
		}
	}

    public bool AlwaysBranches => false;
	public bool SometimesBranches => false;
	public int? BranchTarget => null;
}
