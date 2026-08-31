using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using CILBoy.CIL;

namespace CILBoy.CILParse.Instructions;

public class BOX : CILInstruction {
	public static CILInstructionDefinition Definition = new(0x8C, 4, (args) => new BOX(args));

	public OpCode OpCode => OpCodes.Box;

	public BOX(byte[] args) {
		MetadataToken = BitConverter.ToInt32(args);
	}

	public int MetadataToken { get; set; }

	public byte[] GetBytes() {
		return new byte[]{0x8C}.Concat(BitConverter.GetBytes(MetadataToken)).ToArray();
	}

	public string GetCIL(CILMethodDefinition method) {
		var targetType = method.Factory.GetTypeDefinition(MetadataToken);
		return $"box {targetType.FullName}";
	}

    public void ModifyStack(CILMethodDefinition method, Stack<ISignatureType> current) {
		var targetType = method.Factory.GetTypeDefinition(MetadataToken);
		var type = current.Pop();
		current.Push(new TypeHandleSignatureType(targetType));
	}

    public bool AlwaysBranches => false;
	public bool SometimesBranches => false;
	public int? BranchTarget => null;
}
