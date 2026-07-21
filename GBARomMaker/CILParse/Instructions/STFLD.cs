using System;
using System.Linq;
using System.Reflection.Emit;

namespace GBARomMaker.CILParse.Instructions;

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

	public string GetCIL() {
		return "stfld " + MetadataToken;
	}
}
