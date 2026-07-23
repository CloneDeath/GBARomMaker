using System;
using GBARomMaker.CIL;
using System.Linq;
using System.Reflection.Emit;

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

	public string GetCIL(CILFactory factory) {
		var field = factory.GetFieldDefinition(MetadataToken);
		return $"ldfld {field.FullName}";
	}
}
