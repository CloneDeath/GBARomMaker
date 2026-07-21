using GBARomMaker.ARM;
using GBARomMaker.ARM.ALU;
using GBARomMaker.ARM.Common;
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
		
		var tokenQueue = new TokenQueue(tokens, line);
		var operation = tokenQueue.Dequeue();
		foreach (var handler in _operationMap) {
			if (operation.ToLower().StartsWith(handler.Key)) {
				handler.Value(line, tokenQueue, code);
				return;
			}
		}
		throw new NotImplementedException($"No Handler found for Operation '{operation}'. Line: '{line}'");
	}

	private delegate void AddOperations(string line, TokenQueue tokens, ARMMachineCode code);

	private Dictionary<string, AddOperations> _operationMap = new() {
		{ "nop", (string line, TokenQueue tokens, ARMMachineCode code) => {
			tokens.AssertOperationLength(3);
			tokens.AssertEmpty();
			code.Add(new DataProcessing {
				Operation = ALUOperation.MOV,
				DestinationRegister = 0,
				Op2 = new Register(0)
			});
		}},
		{ "ldr", (string line, TokenQueue tokens, ARMMachineCode code) => {
			tokens.AssertOperationLength(3);
			var destinationRegister = tokens.DequeueRegister();
			tokens.DequeueComma();
			var source = tokens.Dequeue();
			if (source == "=") { // This is actual a psudocommand for MOV/ORs
				var immediate = tokens.DequeueImmediate();
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

				tokens.AssertEmpty();
				return;
			}
			if (source == "[") {
				var baseRegister = tokens.DequeueRegister();
				var next = tokens.Dequeue();
				if (next == "]") {
					throw new NotImplementedException($"Haven't implemented this yet... tokens: {string.Join(" ", tokens)}; line {line}");
				}

				if (next != ",") throw new Exception($"Expected a comma between arguments, got '{next}'. Line '{line}'");

				next = tokens.Dequeue();
				if (next != "#") throw new NotImplementedException("Register Shifted Offsets not supported");
				var immediate = tokens.DequeueSignedImmediate();
				next = tokens.Dequeue();
				if (next != "]") throw new Exception($"Expected a ] to end op, got '{next}'. Line '{line}'");
				tokens.AssertEmpty();

				code.Add(new SingleDataTransfer {
					BaseRegister = baseRegister,
					SourceDestinationRegister = destinationRegister,
					LoadStore = ARM.Common.LoadStore.Load,
					Offset = new GBARomMaker.ARM.Memory.Immediate((uint)Math.Abs(immediate)),
					PrePost = ARM.Common.PrePost.Pre,
					UpDown = immediate < 0 ? UpDown.Down : UpDown.Up,
					WriteBack = false,
					Word = true
				});
				return;
			}
			throw new Exception($"Unexpected token when reading source: '{source}'. Line: '{line}'.");
		}},
		{ "strh", (string line, TokenQueue tokens, ARMMachineCode code) => {
			tokens.AssertOperationLength(4);
			var destinationRegister = tokens.DequeueRegister();
			tokens.DequeueComma();
			
			if (tokens.Dequeue() != "[") throw new Exception("Expected a [ for Address specified");
			var baseRegister = tokens.DequeueRegister();
			var addressNext = tokens.Dequeue();
			if (addressNext != "]") throw new NotImplementedException("Not implemented complex addresses yet...");

			tokens.AssertEmpty();
			code.Add(new MemoryHalf {
				OpCode = HOpCode.STRH,
				DestinationRegister = destinationRegister,
				BaseRegister = baseRegister,
				PrePost = PrePost.Pre,
				ImmediateOffset = 0,
				ImmediateOffsetFlag = true,
				UpDown = UpDown.Up,
				WriteBack = false
			});
		}},
		{ "stm", (string line, TokenQueue tokens, ARMMachineCode code) => {
			LoadBlockDataTransfer(line, tokens, code);
		}},
		{ "ldm", (string line, TokenQueue tokens, ARMMachineCode code) => {
			LoadBlockDataTransfer(line, tokens, code);
		}},
		{ "bx", (string line, TokenQueue tokens, ARMMachineCode code) => {
			tokens.AssertOperationLength(2);
			var register = tokens.DequeueRegister();
			tokens.AssertEmpty();
			code.Add(new BranchExchange {
				OpCode = BranchExchangeOpCode.BX,
				Register = register
			});
		}},
		{ "bl", (string line, TokenQueue tokens, ARMMachineCode code) => {
			Condition condition;
			if (tokens.Operation.Length == 4) {
				condition = ParseCondition(tokens.Operation[2..]);
			} else {
				tokens.AssertOperationLength(2);
				condition = Condition.AL;
			}
			var branchTarget = tokens.Dequeue();
			tokens.AssertEmpty();
			code.AddNeedsLabel(new Branch {
				Instruction = Instruction.BL,
				Condition = condition
			}, branchTarget);
		}},
		{ "b", (string line, TokenQueue tokens, ARMMachineCode code) => {
			Condition condition;
			if (tokens.Operation.Length == 3) {
				condition = ParseCondition(tokens.Operation[1..]);
			} else {
				tokens.AssertOperationLength(1);
				condition = Condition.AL;
			}
			var branchTarget = tokens.Dequeue();
			tokens.AssertEmpty();
			code.AddNeedsLabel(new Branch {
				Instruction = Instruction.B,
				Condition = condition
			}, branchTarget);
		}},
		{ "mul", (string line, TokenQueue tokens, ARMMachineCode code) => {
			tokens.AssertOperationLength(3);
			var destinationRegister = tokens.DequeueRegister();
			tokens.DequeueComma();
			var op1 = tokens.DequeueRegister();
			tokens.DequeueComma();
			var op2 = tokens.DequeueRegister();
			tokens.AssertEmpty();
			code.Add(new Multiply {
				Opcode = MULOperation.MUL,
				DestinationRegister = destinationRegister,
				Op1Register = op1,
				Op2Register = op2
			});
		}},

		// ALU Operations
		{ "and", (string line, TokenQueue tokens, ARMMachineCode code) => {
			LoadALUOperation(line, tokens, code, ALUOperation.AND);
		}},
		{ "eor", (string line, TokenQueue tokens, ARMMachineCode code) => {
			LoadALUOperation(line, tokens, code, ALUOperation.EOR);
		}},
		{ "sub", (string line, TokenQueue tokens, ARMMachineCode code) => {
			LoadALUOperation(line, tokens, code, ALUOperation.SUB);
		}},
		{ "rsb", (string line, TokenQueue tokens, ARMMachineCode code) => {
			LoadALUOperation(line, tokens, code, ALUOperation.RSB);
		}},
		{ "add", (string line, TokenQueue tokens, ARMMachineCode code) => {
			LoadALUOperation(line, tokens, code, ALUOperation.ADD);
		}},
		{ "adc", (string line, TokenQueue tokens, ARMMachineCode code) => {
			LoadALUOperation(line, tokens, code, ALUOperation.ADC);
		}},
		{ "sbc", (string line, TokenQueue tokens, ARMMachineCode code) => {
			LoadALUOperation(line, tokens, code, ALUOperation.SBC);
		}},
		{ "rsc", (string line, TokenQueue tokens, ARMMachineCode code) => {
			LoadALUOperation(line, tokens, code, ALUOperation.RSC);
		}},
		{ "orr", (string line, TokenQueue tokens, ARMMachineCode code) => {
			LoadALUOperation(line, tokens, code, ALUOperation.ORR);
		}},
		{ "bic", (string line, TokenQueue tokens, ARMMachineCode code) => {
			LoadALUOperation(line, tokens, code, ALUOperation.BIC);
		}},

		{ "cmp", (string line, TokenQueue tokens, ARMMachineCode code) => {
			tokens.AssertOperationLength(3);
			var op1 = tokens.DequeueRegister();
			tokens.DequeueComma();
			var op2 = tokens.DequeueAluOp2();
			tokens.AssertEmpty();
			code.Add(new DataProcessing {
				Operation = ALUOperation.CMP,
				SetConditionCodes = true,
				Op1Register = op1,
				Op2 = op2
			});
		}},
		{ "mov", (string line, TokenQueue tokens, ARMMachineCode code) => {
			Condition condition;
			if (tokens.Operation.Length == 5) {
				condition = ParseCondition(tokens.Operation[3..]);
			} else {
				tokens.AssertOperationLength(3);
				condition = Condition.AL;
			}
			var destinationRegister = tokens.DequeueRegister();
			tokens.DequeueComma();
			var op2 = tokens.DequeueAluOp2();
			tokens.AssertEmpty();
			code.Add(new DataProcessing {
				Operation = ALUOperation.MOV,
				DestinationRegister = destinationRegister,
				Op2 = op2,
				Condition = condition
			});
		}},
	};

	public static void LoadALUOperation(string line, TokenQueue tokens, ARMMachineCode code, ALUOperation operation) {
		tokens.AssertOperationLength(3);
		var destinationRegister = tokens.DequeueRegister();
		tokens.DequeueComma();
		var op1 = tokens.DequeueRegister();
		tokens.DequeueComma();
		var op2 = tokens.DequeueAluOp2();
		tokens.AssertEmpty();
		code.Add(new DataProcessing {
			Operation = operation,
			DestinationRegister = destinationRegister,
			Op1Register = op1,
			Op2 = op2
		});
	}

	public static void LoadBlockDataTransfer(string line, TokenQueue tokens, ARMMachineCode code) {//ldmdb sp!, { r0, r1 }
		if (tokens.Operation.Length != 3 && tokens.Operation.Length != 5) {
			throw new NotImplementedException($"Unexpected operation {tokens.Operation}. {line}");
		}
		var loadStore = tokens.Operation.Substring(0, 3) switch {
			"ldm" => LoadStore.Load,
			"stm" => LoadStore.Store,
			_ => throw new NotSupportedException($"Could not interpret Load/Store bit for Block Data Transfer command: {tokens.Operation}, got {tokens.Operation.Substring(0, 3)}")
		};
		var upDown = tokens.Operation.Length > 3 ? tokens.Operation[3] switch {
			'i' => UpDown.Up,
			'd' => UpDown.Down,
			_ => throw new NotSupportedException($"Could not interpret Up/Down bit for Block Data Transfer command: {tokens.Operation}, got {tokens.Operation[4]}")
		} : UpDown.Up;
		var prePost = tokens.Operation.Length > 3 ? tokens.Operation[4] switch {
			'b' => PrePost.Pre,
			'a' => PrePost.Post,
			_ => throw new NotSupportedException($"Could not interpret Pre/Post bit for Block Data Transfer command: {tokens.Operation}, got {tokens.Operation[5]}")
		} : PrePost.Post;

		var baseRegister = tokens.DequeueRegister();
		var next = tokens.Dequeue();
		bool writeback = false;
		if (next == "!") {
			writeback = true;
			next = tokens.Dequeue();
		}
		if (next != ",") {
			throw new Exception($"Unexpected token: '{next}' in '{line}'");
		}

		next = tokens.Dequeue();
		if (next != "{") {
			throw new Exception($"Unexpected token: '{next}' in '{line}'. Expected register list, ie '{{ r0, r1 }}'");
		}

		ushort registerList = 0;
		while (true) {
			var register = tokens.DequeueRegister();
			registerList |= (ushort)(0b1 << register);
			next = tokens.Dequeue();
			if (next == ",") continue;
			if (next == "}") break;
			throw new Exception("Unexpected token when reading list of registers: " + next);
		}
		tokens.AssertEmpty();

		code.Add(new BlockDataTransfer {
			RegisterList = registerList,
			BaseRegister = baseRegister,
			LoadStore = loadStore,
			PrePost = prePost,
			UpDown = upDown,
			WriteBack = writeback
		});
	}

	public static void AssertEmptyQueue(string line, Queue<string> tokens) {
		if (!tokens.Any()) return;
		var extra = string.Join(' ', tokens);
		throw new Exception($"Command not recognized; too many arguments. Got '{extra}'. { line }");
	}

	public static Condition ParseCondition(string condition) {
		return condition.ToUpper() switch {
			"EQ" => Condition.EQ,
			"NE" => Condition.NE,
			
			"CS" => Condition.CS,
			"HS" => Condition.CS,
			
			"CC" => Condition.CC,
			"LO" => Condition.CC,
			
			"MI" => Condition.MI,
			"PL" => Condition.PL,
			"VS" => Condition.VS,
			"VC" => Condition.VC,
			"HI" => Condition.HI,
			"LS" => Condition.LS,
			"GE" => Condition.GE,
			"LT" => Condition.LT,
			"GT" => Condition.GT,
			"LE" => Condition.LE,
			"AL" => Condition.AL,
			"NV" => Condition.NV,
			_ => throw new Exception("Unrecognized Condition " + condition)
		};
	}
} 
