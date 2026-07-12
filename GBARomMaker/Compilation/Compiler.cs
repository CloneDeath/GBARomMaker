using GBARomMaker.Rom.Operations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace GBARomMaker.Compilation;

public class Compiler {
	public Operation[] GetOperationForLine(string line) {
		line = line.Split('@', 2)[0].Trim();
		string[] tokens = Regex
			.Matches(line, @"[A-Za-z_][A-Za-z0-9_]*|0x[0-9A-Fa-f]+|\d+|[^\s]")
			.Select(match => match.Value)
			.ToArray();
		var operation = tokens[0];
		switch (operation.ToLower()) {
			case "ldr": {
				var destinationRegister = int.Parse(tokens[1].Substring(1));
				var seperator = tokens[2];
				if (seperator != ",") throw new Exception("Expected a comma between arguments");
				var source = tokens[3];
				if (source == "=") { // This is actual a psudocommand for MOV/ORRs
					var value = tokens[4];
					uint immediate = value.StartsWith("0x", StringComparison.OrdinalIgnoreCase)
						? Convert.ToUInt32(value[2..], 16)
						: Convert.ToUInt32(value, 10);
					if (immediate == 0) {
						return [new Move {
							DestinationRegister = (byte)destinationRegister,
							ImmediateValue = 0
						}];
					}
					// Find all bytes we need to store...
					var bytes = new List<uint>();
					for (var i = 0; i <= 24; i += 8) {
						var section = (immediate >> i) & 0xFF;
						if (section == 0) continue;

						bytes.Add(section << i);
					}
					if (bytes.Count == 1) {
						return [
							new Move {
								DestinationRegister = (byte)destinationRegister,
								ImmediateValue = immediate
							}
						];
					}

					return new Operation[] {
						new Move {
							DestinationRegister = (byte)destinationRegister,
							ImmediateValue = bytes[0]
						}
					}.Concat(bytes[1..].Select(b => new Or {
						DestinationRegister = (byte)destinationRegister,
						FirstOperandRegister = (byte)destinationRegister,
						ImmediateValue = b
					})).ToArray();
				}
				if (tokens.Length >= 4) throw new Exception("Command not recognized, too many arguments...");
				throw new NotImplementedException(line);
			};
			case "strh": {
				var destinationRegister = int.Parse(tokens[1].Substring(1));
				if (tokens[2] != ",") throw new Exception("Expected a comma between arguments");
				if (tokens[3] != "[") throw new Exception("Expected a [ for Address specified");
				var baseRegister = int.Parse(tokens[4].Substring(1));

				var addressNext = tokens[5];
				
				if (addressNext != "]") throw new NotImplementedException("Not implemented complex addresses yet...");
				if (tokens.Count() > 6) throw new NotImplementedException("Post Index Addressing not implemented");

				return [new MemoryHalf {
					OpCode = HOpCode.STRH,
					DestinationRegister = (byte)destinationRegister,
					BaseRegister = (byte)baseRegister,
					AddOffset = AddOffset.PreTransfer,
					ImmediateOffset = 0,
					ImmediateOffsetFlag = true,
					BaseOperation = BaseOperation.Add,
					WriteBack = false
				}];
			};
			case "mov": {
				var destinationRegister = int.Parse(tokens[1].Substring(1));
				if (tokens[2] != ",") throw new Exception("Expected a comma between arguments");
				if (tokens[3] != "#") throw new Exception("Expected a # for A Literal");
				uint immediate = tokens[4].StartsWith("0x", StringComparison.OrdinalIgnoreCase)
					? Convert.ToUInt32(tokens[4][2..], 16)
					: Convert.ToUInt32(tokens[4], 10);
				if (tokens.Count() > 5) throw new Exception("Too many args passed in...");
				return [new Move {
					DestinationRegister = (byte)destinationRegister,
					Immediate = true,
					ImmediateValue = immediate
				}];
			}
		}
		throw new NotImplementedException(line);
	}
}
