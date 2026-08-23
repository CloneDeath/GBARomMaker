using System;
using CILBoy.CIL;
using System.Reflection.Emit;
using System.Collections.Generic;

namespace CILBoy.CILParse.Instructions;

public class STLOC : ILocationInstruction {
	public static CILInstructionDefinition[] Definitions = [
		new(0x13, 1, (args) => new STLOC_S(args[0])), // stloc.s
		new(0x0A, 0, (_) => new STLOC(0)), // stloc.0
		new(0x0B, 0, (_) => new STLOC(1)), // stloc.1
		new(0x0C, 0, (_) => new STLOC(2)), // stloc.2
		new(0x0D, 0, (_) => new STLOC(3)), // stloc.3
	];

	public OpCode OpCode {
		get {
			return Location switch {
				0 => OpCodes.Stloc_0,
				1 => OpCodes.Stloc_1,
				2 => OpCodes.Stloc_2,
				3 => OpCodes.Stloc_3,
				_ => throw new NotImplementedException()
			};
		}
	}

	public uint Location { get; }

	public STLOC(uint location) {
		if (location > 3) {
			throw new NotImplementedException("Locations greater than 3 not implemented");
		}
		this.Location = location;
	}

    public byte[] GetBytes() {
		var opcode = (byte)(0x0A + Location);
		return [opcode];
    }

    public string GetCIL(CILAssemblyFactory factory, ICILMethod method) {
		return "stloc." + Location;
    }

	public void ModifyStack(CILAssemblyFactory factory, ICILMethod method, Stack<ISignatureType> current) {
		current.Pop();
	}
    public bool AlwaysBranches => false;
	public bool SometimesBranches => false;
	public int? BranchTarget => null;
}

public class STLOC_S : ILocationInstruction {
	public STLOC_S(byte location) {
		Location = location;
	}

	public uint Location { get; }
	public OpCode OpCode => OpCodes.Stloc_S;
	public byte[] GetBytes() => [0x13, (byte)Location];
	public string GetCIL(CILAssemblyFactory factory, ICILMethod method) => $"stloc.s {Location}";

	public void ModifyStack(CILAssemblyFactory factory, ICILMethod method, Stack<ISignatureType> current) {
		current.Pop();
	}
    public bool AlwaysBranches => false;
	public bool SometimesBranches => false;
	public int? BranchTarget => null;
}
