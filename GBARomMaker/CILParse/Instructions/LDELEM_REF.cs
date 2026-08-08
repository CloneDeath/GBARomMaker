using System;
using System.Collections.Generic;
using System.Reflection.Emit;
using System.Reflection.Metadata;
using GBARomMaker.CIL;

namespace GBARomMaker.CILParse.Instructions;

public class LDELEM_REF : CILInstruction {
	public static CILInstructionDefinition Definition = new(0x9A, 0, (_) => new LDELEM_REF());

	public OpCode OpCode => OpCodes.Ldelem_Ref;

    public byte[] GetBytes() {
		return [0x9A];
    }

    public string GetCIL(CILFactory factory, ICILMethod method) {
		return "ldelem.ref";
    }
    
	public void ModifyStack(CILFactory factory, ICILMethod method, Stack<ISignatureType> current) {
		current.Pop(); // index
		var type = current.Pop(); // array
		if (type.Code != SignatureTypeCode.SZArray) {
			throw new Exception($"Attempted to load array element from type {type.Code}");
		}
		var arrayType = (ArraySignatureType)type;
		current.Push(arrayType.InnerType);
	}

    public bool AlwaysBranches => false;
	public bool SometimesBranches => false;
	public int? BranchTarget => null;
}
