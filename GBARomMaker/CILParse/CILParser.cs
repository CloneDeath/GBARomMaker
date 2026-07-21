using System;
using System.Linq;
using System.Collections.Generic;
using GBARomMaker.CILParse.Instructions;
using GBARomMaker.CILParse.FEInstructions;

namespace GBARomMaker.CILParse;

// https://www.ecma-international.org/wp-content/uploads/ECMA-335_6th_edition_june_2012.pdf
public class CILParser {
	public CILParser() {}

	public static readonly List<CILInstructionDefinition> Instructions = new CILInstructionDefinition[][] {
		[ADD.Definition],
		BR.Definitions,
		BRTRUE.Definitions,
		[CALL.Definition],
		[CONV.Definition],
		[DUP.Definition],
		LDARG.Definitions,
		LDC.Definitions,
		LDLOC.Definitions,
		[MUL.Definition],
		[NEWOBJ.Definition],
		[NOP.Definition],
		[RET.Definition],
		[STFLD.Definition],
		[STIND.Definition],
		STLOC.Definitions,
	}.SelectMany(d => d).ToList();
	
	public static readonly List<CILInstructionDefinition> FEInstructions = new CILInstructionDefinition[][] {
		[CEQ.Definition],
		[CGT.Definition],
		[CLT.Definition],
	}.SelectMany(d => d).ToList();

	public CILInstruction[] GetInstructions(byte[] data) {
		var result = new List<CILInstruction>();
		for (var i = 0; i < data.Length; i++) {
			var op = data[i];
			if (op == 0xFE) { // 2-byte op-codes
				i++;
				op = data[i];
				
				var instructionDef = FEInstructions.Find(i => i.OpCode == op);
				if (instructionDef == null) {
					throw new NotImplementedException($"No CIL Instruction Definition Found for 0xFE 0x{op:X2}");
				}
				var args = instructionDef.ArgumentCount > 0
					? data[(i+1) .. (i + 1 + instructionDef.ArgumentCount)]
					: [];
				i += instructionDef.ArgumentCount;

				var instruction = instructionDef.Factory(args);
				result.Add(instruction);
			} else {
				var instructionDef = Instructions.Find(i => i.OpCode == op);
				if (instructionDef == null) {
					throw new NotImplementedException($"No CIL Instruction Definition Found for 0x{op:X2}");
				}
				var args = instructionDef.ArgumentCount > 0
					? data[(i+1) .. (i + 1 + instructionDef.ArgumentCount)]
					: [];
				i += instructionDef.ArgumentCount;

				var instruction = instructionDef.Factory(args);
				result.Add(instruction);
			}
		}
		return result.ToArray();
	}
}
