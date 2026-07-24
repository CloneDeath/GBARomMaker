using System;
using GBARomMaker.CIL;
using System.Linq;
using System.Reflection.Emit;

namespace GBARomMaker.CILParse.Instructions;

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

	public string GetCIL(CILFactory factory) {
		var field = factory.GetFieldDefinition(MetadataToken);
		return $"stsfld {field.FullName}";
	}
}
