using System;
using CILBoy.CIL;
using System.Linq;
using System.Reflection.Emit;
using System.Collections.Generic;

namespace CILBoy.CILParse.Instructions;

public class STSFLD : CILInstruction {
	public static CILInstructionDefinition Definition = new(0x80, 4, (args) => new STSFLD(args));

	public OpCode OpCode => OpCodes.Stsfld;

	public STSFLD(byte[] args) {
		MetadataToken = BitConverter.ToInt32(args);
	}

	public int MetadataToken { get; set; }

	public byte[] GetBytes() {
		return new byte[]{0x80}.Concat(BitConverter.GetBytes(MetadataToken)).ToArray();
	}

	public string GetCIL(CILAssemblyFactory factory, ICILMethod method) {
		var field = factory.GetFieldDefinition(MetadataToken);
		return $"stsfld {field.FullName}";
	}

	public void ModifyStack(CILAssemblyFactory factory, ICILMethod method, Stack<ISignatureType> current) {
		current.Pop();
	}
    public bool AlwaysBranches => false;
	public bool SometimesBranches => false;
	public int? BranchTarget => null;
}
