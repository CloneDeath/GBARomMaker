using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using CILBoy.CIL;

namespace CILBoy.CILParse.Instructions;

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

	public string GetCIL(CILMethodDefinition method) {
		var targetMethod = method.Factory.GetMethodDefinition(MetadataToken);
		var types = string.Join(", ", targetMethod.GetArgumentTypes());
		return $"call {targetMethod.ReturnType} {targetMethod.FullName}({types})";
	}

    public void ModifyStack(CILMethodDefinition method, Stack<ISignatureType> current) {
		var targetMethod = method.Factory.GetMethodDefinition(MetadataToken);
		var args = targetMethod.ParameterCount + (targetMethod.IsInstance ? 1 : 0);
		for (var i = 0; i < args; i++) {
			current.Pop();
		}
		if (targetMethod.HasReturnValue) {
			current.Push(targetMethod.ReturnType);
		}
	}

    public bool AlwaysBranches => false;
	public bool SometimesBranches => false;
	public int? BranchTarget => null;
}
