using System;
using System.Reflection.Emit;

namespace GBARomMaker.CILParse.Instructions;

public class STLOC : CILInstruction {
	public static CILInstructionDefinition[] Definitions = [
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

    public string GetCIL() {
		return "stloc." + Location;
    }
}
