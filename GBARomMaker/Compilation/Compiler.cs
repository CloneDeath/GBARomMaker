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

		if (tokens.Length == 0) return; // empty line
		
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
			tokens.Operation.DequeueValue("nop");
			tokens.Operation.AssertEmpty();
			tokens.AssertEmpty();
			code.Add(new DataProcessing {
				Operation = ALUOperation.MOV,
				DestinationRegister = 0,
				Op2 = new Register(0)
			});
		}},
		{ "swi", (string line, TokenQueue tokens, ARMMachineCode code) => {
			tokens.Operation.DequeueValue("swi");
			var condition = tokens.Operation.DequeueCondition();
			tokens.Operation.AssertEmpty();
			
			var comment = tokens.DequeueImmediate();
			tokens.AssertEmpty();

			code.Add(new SoftwareInterrupt {
				Condition = condition,
				Comment = comment
			});
		}},
		{ "ldr", (string line, TokenQueue tokens, ARMMachineCode code) => {
			tokens.Operation.DequeueValue("ldr");
			var condition = tokens.Operation.DequeueCondition();
			var flag = tokens.Operation.TryDequeue(1, out var f) ? f : null;
			tokens.Operation.AssertEmpty();

			var destinationRegister = tokens.DequeueRegister();
			tokens.DequeueComma();
			var source = tokens.Dequeue();
			if (source == "=") { // This is actual a psudocommand for MOV/ORs or PC Offset to Label
				if (flag != null) throw new NotImplementedException($"Tried to direct assign to ldrh, doesn't make sense... Line '{line}'");
				var upcoming = tokens.Peek();
				if (!new Regex("\\d").IsMatch(upcoming)) {
					var label = tokens.Dequeue();

					code.AddNeedsLabel(new DataProcessingWithLabelOffset {
						Condition = condition,
						Operation = ALUOperation.ADD,
						DestinationRegister = destinationRegister,
						Op1Register = 15, // PC
						SetConditionCodes = false
					}, label);

					tokens.AssertEmpty();
					return;
				}

				var immediateValue = tokens.DequeueImmediate();
				if (immediateValue == 0) {
					code.Add(new DataProcessing {
						Operation = ALUOperation.MOV,
						DestinationRegister = destinationRegister,
						Op2 = new Immediate(0),
						Condition = condition
					});
					tokens.AssertEmpty();
					return;
				}
				// Find all bytes we need to store...
				var bytes = new List<uint>();
				for (var i = 0; i <= 24; i += 8) {
					var section = (immediateValue >> i) & 0xFF;
					if (section == 0) continue;

					bytes.Add(section << i);
				}
				if (bytes.Count == 1) {
					code.Add(new DataProcessing {
						Operation = ALUOperation.MOV,
						DestinationRegister = destinationRegister,
						Op2 = new Immediate(immediateValue),
						Condition = condition
					});
					tokens.AssertEmpty();
					return;
				}

				code.Add(new DataProcessing {
					Operation = ALUOperation.MOV,
					DestinationRegister = destinationRegister,
					Op2 = new Immediate(bytes[0]),
					Condition = condition
				});
				code.Add(bytes[1..].Select(b => new DataProcessing {
					Operation = ALUOperation.ORR,
					DestinationRegister = destinationRegister,
					Op1Register = destinationRegister,
					Op2 = new Immediate(b),
					Condition = condition
				}).ToArray());

				tokens.AssertEmpty();
				return;
			}
			if (source != "[") {
				throw new Exception($"Unexpected token when reading source: '{source}'. Line: '{line}'.");
			}
			var baseRegister = tokens.DequeueRegister();
			var next = tokens.Dequeue();
			if (next == "]") {
				tokens.AssertEmpty();
				code.Add(flag switch {
					null => new SingleDataTransfer {
						Condition = condition,
						BaseRegister = baseRegister,
						SourceDestinationRegister = destinationRegister,
						LoadStore = LoadStore.Load,
						Offset = new ARM.Memory.Immediate(0),
						PrePost = PrePost.Pre,
						UpDown = UpDown.Up,
						WriteBack = false,
						Word = true
					},
					"h" => new MemoryHalf {
						Condition = condition,
						BaseRegister = baseRegister,
						SourceDestinationRegister = destinationRegister,
						OpCode = HOpCode.LDRH,
						ImmediateOffsetFlag = true,
						ImmediateOffset = 0,
						PrePost = PrePost.Pre,
						UpDown = UpDown.Up,
						WriteBack = false
					},
					_ => throw new Exception($"Unexpected flag '{flag}'. Line '{line}'")
				});
				return;
			}

			if (next != ",") throw new Exception($"Expected a comma between arguments, got '{next}'. Line '{line}'");

			next = tokens.Dequeue();
			if (next != "#") throw new NotImplementedException("Register Shifted Offsets not supported");
			var immediate = tokens.DequeueSignedImmediate();
			next = tokens.Dequeue();
			if (next != "]") throw new Exception($"Expected a ] to end op, got '{next}'. Line '{line}'");
			tokens.AssertEmpty();

			code.Add(flag switch {
				null => new SingleDataTransfer {
					Condition = condition,
					BaseRegister = baseRegister,
					SourceDestinationRegister = destinationRegister,
					LoadStore = ARM.Common.LoadStore.Load,
					Offset = new ARM.Memory.Immediate((uint)Math.Abs(immediate)),
					PrePost = PrePost.Pre,
					UpDown = immediate < 0 ? UpDown.Down : UpDown.Up,
					WriteBack = false,
					Word = true
				},
				"h" => new MemoryHalf {
					Condition = condition,
					BaseRegister = baseRegister,
					SourceDestinationRegister = destinationRegister,
					OpCode = HOpCode.LDRH,
					ImmediateOffsetFlag = true,
					ImmediateOffset = (byte)Math.Abs(immediate),
					PrePost = PrePost.Pre,
					UpDown = immediate < 0 ? UpDown.Down : UpDown.Up,
					WriteBack = false
				},
				_ => throw new Exception($"Unexpected flag '{flag}'. Line '{line}'")
			});
			return;
		}},
		{ "str", (string line, TokenQueue tokens, ARMMachineCode code) => {
			tokens.Operation.DequeueValue("str");
			var condition = tokens.Operation.DequeueCondition();
			var flag = tokens.Operation.TryDequeue(1, out var f) ? f : null;
			tokens.Operation.AssertEmpty();

			var sourceRegister = tokens.DequeueRegister();
			tokens.DequeueComma();
			if (tokens.Dequeue() != "[") throw new Exception($"Expected [ for addr. Line '{line}'");
			var baseRegister = tokens.DequeueRegister();

			var next = tokens.Dequeue();
			if (next == "]") {
				tokens.AssertEmpty();
				code.Add(flag == "h" 
					? new MemoryHalf {
						Condition = condition,
						SourceDestinationRegister = sourceRegister,
						BaseRegister = baseRegister,
						OpCode = HOpCode.STRH,
						PrePost = PrePost.Pre,
						ImmediateOffset = 0,
						ImmediateOffsetFlag = true,
						UpDown = UpDown.Up,
						WriteBack = false
					}
					: new SingleDataTransfer {
						Condition = condition,
						SourceDestinationRegister = sourceRegister,
						BaseRegister = baseRegister,
						LoadStore = LoadStore.Store,
						Offset = new ARM.Memory.Immediate(0),
						PrePost = PrePost.Pre,
						UpDown = UpDown.Up,
						WriteBack = false,
						Word = flag == "b" ? false : true
					}
				);
				return;
			}
			if (next != ",") throw new Exception($"Expected a comma between arguments, got '{next}'. Line '{line}'");
			next = tokens.Dequeue();
			if (next != "#") throw new NotImplementedException($"Register Shifted Offsets not supported. Line '{line}'");
			var immediate = tokens.DequeueSignedImmediate();
			next = tokens.Dequeue();
			if (next != "]") throw new Exception($"Expected a ] to end op, got '{next}'. Line '{line}'");
			tokens.AssertEmpty();

			code.Add(flag == "h"
				? new MemoryHalf {
					Condition = condition,
					SourceDestinationRegister = sourceRegister,
					BaseRegister = baseRegister,
					OpCode = HOpCode.STRH,
					PrePost = PrePost.Pre,
					ImmediateOffset = (byte)immediate,
					ImmediateOffsetFlag = true,
					UpDown = immediate < 0 ? UpDown.Down : UpDown.Up,
					WriteBack = false
				}
				: new SingleDataTransfer {
					Condition = condition,
					SourceDestinationRegister = sourceRegister,
					BaseRegister = baseRegister,
					LoadStore = LoadStore.Store,
					UpDown = immediate < 0 ? UpDown.Down : UpDown.Up,
					Offset = new ARM.Memory.Immediate((uint)Math.Abs(immediate)),
					PrePost = ARM.Common.PrePost.Pre,
					WriteBack = false,
					Word = flag == "b" ? false : true
				}
			);
		}},
		{ "stm", (string line, TokenQueue tokens, ARMMachineCode code) => {
			LoadBlockDataTransfer("stm", line, tokens, code);
		}},
		{ "ldm", (string line, TokenQueue tokens, ARMMachineCode code) => {
			LoadBlockDataTransfer("ldm", line, tokens, code);
		}},
		{ "push", (string line, TokenQueue tokens, ARMMachineCode code) => {
			LoadBlockDataTransfer("push", line, tokens, code);
		}},
		{ "pop", (string line, TokenQueue tokens, ARMMachineCode code) => {
			LoadBlockDataTransfer("pop", line, tokens, code);
		}},
		{ "bx", (string line, TokenQueue tokens, ARMMachineCode code) => {
			tokens.Operation.DequeueValue("bx");
			var condition = tokens.Operation.DequeueCondition();
			tokens.Operation.AssertEmpty();

			var register = tokens.DequeueRegister();
			tokens.AssertEmpty();
			code.Add(new BranchExchange {
				OpCode = BranchExchangeOpCode.BX,
				Register = register,
				Condition = condition
			});
		}},
		{ "b", (string line, TokenQueue tokens, ARMMachineCode code) => {
			tokens.Operation.DequeueValue("b");
			var condition = tokens.Operation.DequeueCondition();
			var gotCondition = tokens.Operation.Index == 3;
			Instruction instruction = Instruction.B;
			if (!gotCondition) {
				if (tokens.Operation.TryDequeue(1, out var flag) && flag == "l") {
					instruction = Instruction.BL;
					condition = tokens.Operation.DequeueCondition();
				}
			}
			tokens.Operation.AssertEmpty();

			var branchTarget = tokens.Dequeue();
			tokens.AssertEmpty();
			code.AddNeedsLabel(new Branch {
				Instruction = instruction,
				Condition = condition
			}, branchTarget);
		}},
		{ "mul", (string line, TokenQueue tokens, ARMMachineCode code) => {
			tokens.Operation.DequeueValue("mul");
			var condition = tokens.Operation.DequeueCondition();
			tokens.Operation.AssertEmpty();

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
				Op2Register = op2,
				Condition = condition
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
			tokens.Operation.DequeueValue("cmp");
			var condition = tokens.Operation.DequeueCondition();
			tokens.Operation.AssertEmpty();

			var op1 = tokens.DequeueRegister();
			tokens.DequeueComma();
			var op2 = tokens.DequeueAluOp2();
			tokens.AssertEmpty();
			code.Add(new DataProcessing {
				Operation = ALUOperation.CMP,
				SetConditionCodes = true,
				Op1Register = op1,
				Op2 = op2,
				Condition = condition
			});
		}},
		{ "mov", (string line, TokenQueue tokens, ARMMachineCode code) => {
			tokens.Operation.DequeueValue("mov");
			var condition = tokens.Operation.DequeueCondition();
			tokens.Operation.AssertEmpty();

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
		{ "mvn", (string line, TokenQueue tokens, ARMMachineCode code) => {
			tokens.Operation.DequeueValue("mvn");
			var condition = tokens.Operation.DequeueCondition();
			tokens.Operation.AssertEmpty();

			var destinationRegister = tokens.DequeueRegister();
			tokens.DequeueComma();
			var op2 = tokens.DequeueAluOp2();
			tokens.AssertEmpty();
			code.Add(new DataProcessing {
				Operation = ALUOperation.MVN,
				DestinationRegister = destinationRegister,
				Op2 = op2,
				Condition = condition
			});
		}},
		{ "lsl", (string line, TokenQueue tokens, ARMMachineCode code) => {
			tokens.Operation.DequeueValue("lsl");
			var condition = tokens.Operation.DequeueCondition();
			tokens.Operation.AssertEmpty();

			var destinationRegister = tokens.DequeueRegister();
			tokens.DequeueComma();
			var op2Register = tokens.DequeueRegister();
			tokens.DequeueComma();
			var shiftRegister = tokens.DequeueRegister();
			tokens.AssertEmpty();
			code.Add(new DataProcessing {
				Operation = ALUOperation.MOV,
				DestinationRegister = destinationRegister,
				Condition = condition,
				Op2 = new ARM.ALU.Register (op2Register) {
					ShiftRegister = shiftRegister,
					ShiftType = ShiftType.LSL,
					ShiftByRegister = true
				}
			});
		}},
	};

	public static void LoadALUOperation(string line, TokenQueue tokens, ARMMachineCode code, ALUOperation operation) {
		tokens.Operation.DequeueValue(operation.ToString());
		var condition = tokens.Operation.DequeueCondition();
		tokens.Operation.AssertEmpty();

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
			Op2 = op2,
			Condition = condition
		});
	}

	public static void LoadBlockDataTransfer(string operation, string line, TokenQueue tokens, ARMMachineCode code) {//ldmdb sp!, { r0, r1 }
		tokens.Operation.DequeueValue(operation);
		var condition = tokens.Operation.DequeueCondition();

		LoadStore loadStore = operation switch {
			"ldm" => LoadStore.Load,
			"pop" => LoadStore.Load,
			"stm" => LoadStore.Store,
			"push" => LoadStore.Store,
			_ => throw new NotSupportedException($"Could not interpret Load/Store bit for Block Data Transfer command. Line '{line}'")
		};

		var next = tokens.Operation.TryDequeue(1, out var ud) ? ud : null;
		UpDown upDown;
		if (operation == "pop" || next == "i") {
			upDown = UpDown.Up;
		} else if (operation == "push" || next == "d") {
			upDown = UpDown.Down;
		} else {
			throw new NotSupportedException($"Could not interpret Up/Down bit for Block Data Transfer command. Line '{line}'");
		}

		next = tokens.Operation.TryDequeue(1, out var pp) ? pp: null;
		PrePost prePost;
		if (operation == "push" || next == "b") {
			prePost = PrePost.Pre;
		} else if (operation == "pop" || next == "a") {
			prePost = PrePost.Post;
		} else {
			throw new NotSupportedException($"Could not interpret Pre/Post bit for Block Data Transfer command. Line '{line}'");
		}
		tokens.Operation.AssertEmpty();

		var baseRegister = tokens.DequeueRegister();
		next = tokens.Dequeue();
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
} 
