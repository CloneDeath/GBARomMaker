using System;
using GBARomMaker.CIL;
using System.Reflection.Emit;
using System.Collections.Generic;

namespace GBARomMaker.CILParse.Instructions;


public class LDLOC : ILocationInstruction {
	public static CILInstructionDefinition[] Definitions = [
		new(0x11, 1, (args) => new LDLOC_S(args[0])), // ldloc.s
		new(0x06, 0, (_) => new LDLOC(0)), // ldloc.0
		new(0x07, 0, (_) => new LDLOC(1)), // ldloc.1
		new(0x08, 0, (_) => new LDLOC(2)), // ldloc.2
		new(0x09, 0, (_) => new LDLOC(3)), // ldloc.3
	];

	public OpCode OpCode {
		get {
			return Location switch {
				0 => OpCodes.Ldloc_0,
				1 => OpCodes.Ldloc_1,
				2 => OpCodes.Ldloc_2,
				3 => OpCodes.Ldloc_3,
				_ => throw new NotImplementedException()
			};
		}
	}

	public uint Location { get; }

	public LDLOC(uint location) {
		if (location > 3) {
			throw new NotImplementedException("Locations greater than 3 not implemented");
		}
		this.Location = location;
	}

    public byte[] GetBytes() {
		var opcode = (byte)(0x06 + Location);
		return [opcode];
    }

    public string GetCIL(CILFactory factory, ICILMethod method) {
		return $"ldloc.{Location} // {method.GetLocalVariableTypes()[Location]}";
    }
    
	public void ModifyStack(CILFactory factory, ICILMethod method, Stack<ISignatureType> current) {
		current.Push(method.GetLocalVariableTypes()[Location]);
	}

    public bool AlwaysBranches => false;
	public bool SometimesBranches => false;
	public int? BranchTarget => null;
}

public class LDLOC_S : ILocationInstruction {
	public LDLOC_S(byte location) {
		Location = location;
	}

	public uint Location { get; }

	public OpCode OpCode => OpCodes.Ldloc_S;

	public byte[] GetBytes() => [0x11, (byte)Location];

	public string GetCIL(CILFactory factory, ICILMethod method) => $"ldloc.s {Location} // {method.GetLocalVariableTypes()[Location]}";
    
	public void ModifyStack(CILFactory factory, ICILMethod method, Stack<ISignatureType> current) {
		current.Push(method.GetLocalVariableTypes()[Location]);
	}

    public bool AlwaysBranches => false;
	public bool SometimesBranches => false;
	public int? BranchTarget => null;
}
