using GBARomMaker.ARM;
using GBARomMaker.ARM.ALU;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace GBARomMaker.Compilation;

public class Compiler {
	public ARMMachineCode GetOperationsForAssembly(params string[] lines) {
		var code = new ARMMachineCode();
		foreach (var line in lines) {
			AddOperationsForAssembly(line, code);
		}
		return code;
	}

	public void AddOperationsForAssembly(string line, ARMMachineCode code) {
		line = line.Split('@', 2)[0].Trim();
		string[] tokens = Regex
			.Matches(line, @"[\w]+|[^\s]")
			.Select(match => match.Value)
			.ToArray();

		if (tokens.Length == 2 && tokens[1] == ":") {
			var label = tokens[0];
			code.AddLabel(label);
			return;
		}
		
		var operation = tokens[0];
		foreach (var handler in _operationMap) {
			if (operation.StartsWith(handler.Key)) {
				handler.Value(line, tokens, code);
				return;
			}
		}
		throw new NotImplementedException($"No Handler found for Operation '{operation}'. Line: '{line}'");
	}

	private delegate void AddOperations(string line, string[] tokens, ARMMachineCode code);

	private Dictionary<string, AddOperations> _operationMap = new() {
		{ "nop", (string line, string[] tokens, ARMMachineCode code) => {
			code.Add(new DataProcessing {
				Operation = ALUOperation.MOV,
				DestinationRegister = 0,
				Op2 = new Register(0)
			});
		}},
		{ "ldr", (string line, string[] tokens, ARMMachineCode code) => {
			var tokenList = new Queue<string>(tokens.Skip(1).ToList());
			var destinationRegister = ParseRegister(tokenList.Dequeue());
			var seperator = tokenList.Dequeue();
			if (seperator != ",") throw new Exception("Expected a comma between arguments");
			var source = tokenList.Dequeue();
			if (source == "=") { // This is actual a psudocommand for MOV/ORs
				var value = tokenList.Dequeue();
				uint immediate = value.StartsWith("0x", StringComparison.OrdinalIgnoreCase)
					? Convert.ToUInt32(value[2..], 16)
					: Convert.ToUInt32(value, 10);
				if (immediate == 0) {
					code.Add(new DataProcessing {
						Operation = ALUOperation.MOV,
						DestinationRegister = destinationRegister,
						Op2 = new Immediate(0)
					});
					return;
				}
				// Find all bytes we need to store...
				var bytes = new List<uint>();
				for (var i = 0; i <= 24; i += 8) {
					var section = (immediate >> i) & 0xFF;
					if (section == 0) continue;

					bytes.Add(section << i);
				}
				if (bytes.Count == 1) {
					code.Add(new DataProcessing {
						Operation = ALUOperation.MOV,
						DestinationRegister = destinationRegister,
						Op2 = new Immediate(immediate)
					});
					return;
				}

				code.Add(new DataProcessing {
					Operation = ALUOperation.MOV,
					DestinationRegister = destinationRegister,
					Op2 = new Immediate(bytes[0])
				});
				code.Add(bytes[1..].Select(b => new DataProcessing {
					Operation = ALUOperation.ORR,
					DestinationRegister = destinationRegister,
					Op1Register = destinationRegister,
					Op2 = new Immediate(b)
				}).ToArray());
				return;
			}
			if (tokenList.Any()) throw new Exception("Command not recognized, too many arguments...");
			throw new NotImplementedException(line);
		}},
		{ "strh", (string line, string[] tokens, ARMMachineCode code) => {
			var tokenList = new Queue<string>(tokens.Skip(1).ToList());
			var destinationRegister = ParseRegister(tokenList.Dequeue());
			if (tokenList.Dequeue() != ",") throw new Exception("Expected a comma between arguments");
			if (tokenList.Dequeue() != "[") throw new Exception("Expected a [ for Address specified");
			var baseRegister = ParseRegister(tokenList.Dequeue());

			var addressNext = tokens[5];
			
			if (addressNext != "]") throw new NotImplementedException("Not implemented complex addresses yet...");
			if (tokens.Count() > 6) throw new NotImplementedException("Post Index Addressing not implemented");

			code.Add(new MemoryHalf {
				OpCode = HOpCode.STRH,
				DestinationRegister = destinationRegister,
				BaseRegister = baseRegister,
				AddOffset = AddOffset.PreTransfer,
				ImmediateOffset = 0,
				ImmediateOffsetFlag = true,
				BaseOperation = BaseOperation.Add,
				WriteBack = false
			});
		}},
		{ "mov", (string line, string[] tokens, ARMMachineCode code) => {
			var tokenList = new Queue<string>(tokens.Skip(1).ToList());
			var destinationRegister = ParseRegister(tokenList.Dequeue());
			if (tokenList.Dequeue() != ",") throw new Exception("Expected a comma between arguments");
			if (tokenList.Dequeue() != "#") throw new Exception("Expected a # for A Literal");
			var next = tokenList.Dequeue();
			uint immediate = next.StartsWith("0x", StringComparison.OrdinalIgnoreCase)
				? Convert.ToUInt32(next[2..], 16)
				: Convert.ToUInt32(next, 10);
			if (tokenList.Any()) throw new Exception("Too many args passed in...");
			code.Add(new DataProcessing {
				Operation = ALUOperation.MOV,
				DestinationRegister = destinationRegister,
				Op2 = new Immediate(immediate)
			});
		}},
		{ "stm", (string line, string[] tokens, ARMMachineCode code) => {
			LoadBlockDataTransfer(tokens, code);
		}},
		{ "ldm", (string line, string[] tokens, ARMMachineCode code) => {
			LoadBlockDataTransfer(tokens, code);
		}},
		{ "bx", (string line, string[] tokens, ARMMachineCode code) => {
			var tokenList = new Queue<string>(tokens.Skip(1).ToList());
			var register = ParseRegister(tokenList.Dequeue());
			if (tokenList.Any()) throw new Exception("Too many arguments for bx operation " + line);
			code.Add(new BranchExchange {
				OpCode = BranchExchangeOpCode.BX,
				Register = register
			});
		}},
		{ "b", (string line, string[] tokens, ARMMachineCode code) => {
			var tokenList = new Queue<string>(tokens.Skip(1).ToList());
			var branchTarget = tokenList.Dequeue();
			if (tokenList.Any()) throw new Exception("Too many arguments for b operation " + line);
			code.Add(new Branch {
				Instruction = Instruction.B
			});
		}},
		{ "mul", (string line, string[] tokens, ARMMachineCode code) => {
			var tokenList = new Queue<string>(tokens.Skip(1).ToList());
			throw new NotImplementedException();

		}},
	};

	public static void LoadBlockDataTransfer(string[] tokens, ARMMachineCode code) {//ldmdb sp!, { r0, r1 }
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
		while (true) {
			next = tokenList.Dequeue();
			var register = ParseRegister(next);
			registerList |= (ushort)(0b1 << register);
			next = tokenList.Dequeue();
			if (next == ",") continue;
			if (next == "}") break;
			throw new Exception("Unexpected token when reading list of registers: " + next);
		}
		if (tokenList.Any()) throw new Exception("Got more tokens than expected");

		var operation = tokens[0];
		var loadStore = operation.Substring(0, 3) switch {
			"ldm" => LoadStore.Load,
			"stm" => LoadStore.Store,
			_ => throw new NotSupportedException($"Could not interpret Load/Store bit for Block Data Transfer command: {operation}, got {operation.Substring(0, 3)}")
		};
		var upDown = operation[3] switch {
			'i' => UpDown.Up,
			'd' => UpDown.Down,
			_ => throw new NotSupportedException($"Could not interpret Up/Down bit for Block Data Transfer command: {operation}, got {operation[4]}")
		};
		var prePost = operation[4] switch {
			'b' => PrePost.Pre,
			'a' => PrePost.Post,
			_ => throw new NotSupportedException($"Could not interpret Pre/Post bit for Block Data Transfer command: {operation}, got {operation[5]}")
		};
		code.Add(new BlockDataTransfer {
			RegisterList = registerList,
			BaseRegister = baseRegister,
			LoadStore = loadStore,
			PrePost = prePost,
			UpDown = upDown,
			WriteBack = writeback
		});
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
