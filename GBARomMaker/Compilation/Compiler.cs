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
				var destinationRegister = ParseRegister(tokens[1]);
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
							DestinationRegister = destinationRegister,
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
								DestinationRegister = destinationRegister,
								ImmediateValue = immediate
							}
						];
					}

					return new Operation[] {
						new Move {
							DestinationRegister = destinationRegister,
							ImmediateValue = bytes[0]
						}
					}.Concat(bytes[1..].Select(b => new Or {
						DestinationRegister = destinationRegister,
						FirstOperandRegister = destinationRegister,
						ImmediateValue = b
					})).ToArray();
				}
				if (tokens.Length >= 4) throw new Exception("Command not recognized, too many arguments...");
				throw new NotImplementedException(line);
			};
			case "strh": {
				var destinationRegister = ParseRegister(tokens[1]);
				if (tokens[2] != ",") throw new Exception("Expected a comma between arguments");
				if (tokens[3] != "[") throw new Exception("Expected a [ for Address specified");
				var baseRegister = ParseRegister(tokens[4]);

				var addressNext = tokens[5];
				
				if (addressNext != "]") throw new NotImplementedException("Not implemented complex addresses yet...");
				if (tokens.Count() > 6) throw new NotImplementedException("Post Index Addressing not implemented");

				return [new MemoryHalf {
					OpCode = HOpCode.STRH,
					DestinationRegister = destinationRegister,
					BaseRegister = baseRegister,
					AddOffset = AddOffset.PreTransfer,
					ImmediateOffset = 0,
					ImmediateOffsetFlag = true,
					BaseOperation = BaseOperation.Add,
					WriteBack = false
				}];
			};
			case "mov": {
				var destinationRegister = ParseRegister(tokens[1]);
				if (tokens[2] != ",") throw new Exception("Expected a comma between arguments");
				if (tokens[3] != "#") throw new Exception("Expected a # for A Literal");
				uint immediate = tokens[4].StartsWith("0x", StringComparison.OrdinalIgnoreCase)
					? Convert.ToUInt32(tokens[4][2..], 16)
					: Convert.ToUInt32(tokens[4], 10);
				if (tokens.Count() > 5) throw new Exception("Too many args passed in...");
				return [new Move {
					DestinationRegister = destinationRegister,
					Immediate = true,
					ImmediateValue = immediate
				}];
			}
			case "stmia": {
				var tokenList = new Queue<string>(tokens.Skip(1).ToList());
				var baseRegister = ParseRegister(tokenList.Dequeue());
				var next = tokenList.Dequeue();
				bool writeback = false;
				if (next == "!") {
					writeback = true;
					next = tokenList.Dequeue();
				}
				if (next != ",") {
				 	throw new Exception("Unexpected token: " + next);
				}
				
				next = tokenList.Dequeue();
				if (next != "{") {
					throw new Exception("Unexpected token: " + next);
				}

				ushort registerList = 0;
				while (next != "}") {
					next = tokenList.Dequeue();
					if (next == "}") break;
					var register = ParseRegister(next);
					registerList |= (ushort)(0b1 << register);
				}
				if (tokenList.Any()) throw new Exception("Got more tokens than expected");
				return [new BlockDataTransfer {
					RegisterList = registerList,
					BaseRegister = baseRegister,
					LoadStore = LoadStore.Store,
					PrePost = PrePost.Post,
					UpDown = UpDown.Up,
					WriteBack = writeback
				}];
			}
		}
		throw new NotImplementedException(line);
	}

	public static byte ParseRegister(string register) {
		return register switch {
			"sp" => 13,
			"lr" => 14,
			"pc" => 15,
			_ => byte.TryParse(register.Substring(1), out var r) ? r : throw new Exception("Failed to parse " + register)
		};
	}
}
