using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using System.Reflection.Metadata;
using GBARomMaker.CIL;

namespace GBARomMaker.CILParse.Instructions;

public class LDSTR : CILInstruction {
	public static CILInstructionDefinition Definition = new(0x72, 4, (args) => new LDSTR(args));

	public OpCode OpCode => OpCodes.Ldstr;

	public LDSTR(byte[] args) {
		MetadataToken = BitConverter.ToInt32(args);
	}

	public int MetadataToken { get; set; }

	public string GetString(CILFactory factory) => factory.GetString(MetadataToken);

	public byte[] GetBytes() {
		return new byte[]{0x72}.Concat(BitConverter.GetBytes(MetadataToken)).ToArray();
	}

	public string GetCIL(CILFactory factory, ICILMethod method) {
		var str = factory.GetString(MetadataToken);
		return $"ldstr \"{str}\"";
	}
    
	public void ModifyStack(CILFactory factory, ICILMethod method, Stack<ISignatureType> current) {
		current.Push(new SignatureType(SignatureTypeCode.String));
	}

    public bool AlwaysBranches => false;
	public bool SometimesBranches => false;
	public int? BranchTarget => null;
}
