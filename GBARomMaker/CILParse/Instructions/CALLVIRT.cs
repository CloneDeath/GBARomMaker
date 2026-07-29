using System;
using System.Linq;
using System.Reflection.Emit;
using GBARomMaker.CIL;

namespace GBARomMaker.CILParse.Instructions;

public class CALLVIRT : CILInstruction {
	public static CILInstructionDefinition Definition = new(0x6F, 4, (args) => new CALLVIRT(args));

	public OpCode OpCode => OpCodes.Callvirt;

	public CALLVIRT(byte[] args) {
		MetadataToken = BitConverter.ToInt32(args);
	}

	public int MetadataToken { get; set; }

	public byte[] GetBytes() {
		return new byte[]{0x6F}.Concat(BitConverter.GetBytes(MetadataToken)).ToArray();
	}

	public string GetCIL(CILFactory factory) {
		var method = factory.GetMethodDefinition(MetadataToken);
		return "callvirt " + method.FullName;
	}
}
