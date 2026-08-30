using System;
using CILBoy.CIL;
using System.Linq;
using System.Reflection.Emit;
using System.Collections.Generic;

namespace CILBoy.CILParse.Instructions;

public class STFLD : CILInstruction {
	public static CILInstructionDefinition Definition = new(0x7D, 4, (args) => new STFLD(args));

	public OpCode OpCode => OpCodes.Stfld;

	public STFLD(byte[] args) {
		MetadataToken = BitConverter.ToInt32(args);
	}

	public int MetadataToken { get; set; }

	public byte[] GetBytes() {
		return new byte[]{0x7D}.Concat(BitConverter.GetBytes(MetadataToken)).ToArray();
	}

	public string GetCIL(ICILMethod method) {
		var field = method.Factory.GetFieldDefinition(MetadataToken);
		return "stfld " + field.FullName;
	}

	public void ModifyStack(ICILMethod method, Stack<ISignatureType> current) {
		current.Pop();
		current.Pop();
	}
    public bool AlwaysBranches => false;
	public bool SometimesBranches => false;
	public int? BranchTarget => null;
}
