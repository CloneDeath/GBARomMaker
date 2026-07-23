using System;
using System.Linq;
using System.Reflection.Emit;
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

	public string GetCIL(CILFactory factory) {
		var method = factory.GetMethodDefinition(MetadataToken);
		return "call " + method.FullName;
	}
}
