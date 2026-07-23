using System;
using GBARomMaker.CIL;
using System.Reflection.Emit;

namespace GBARomMaker.CILParse.Instructions;

public class LDLOC : CILInstruction {
	public static CILInstructionDefinition[] Definitions = [
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

    public string GetCIL(CILFactory factory) {
		return "ldloc." + Location;
    }
}
