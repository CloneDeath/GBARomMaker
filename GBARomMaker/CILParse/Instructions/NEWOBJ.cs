using System;
using System.Linq;
using System.Reflection.Emit;

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

	public string GetCIL() {
		return "newobj " + MetadataToken;
	}
}
