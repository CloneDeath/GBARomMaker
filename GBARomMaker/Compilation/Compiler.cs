using GBARomMaker.ARM;
using GBARomMaker.ARM.Common;
using GBARomMaker.Compilation.Operations;
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

		if (tokens.Length >= 2 && tokens[0] == "." && tokens[1] == "word") {
			var wordQueue = new TokenQueue(tokens, line);
			wordQueue.DequeueToken(".");
			wordQueue.DequeueToken("word");
			var immediate = wordQueue.DequeueImmediate();
			wordQueue.AssertEmpty();
			code.Add(new Word {
				Value = BitConverter.GetBytes(immediate)
			});
			return;
		}

		if (tokens.Length == 0) return; // empty line
		
		var tokenQueue = new TokenQueue(tokens, line);
		var operation = tokenQueue.Dequeue();

		var operations = new List<IOperationAssembler> {
			new Ldr(),
			new Nop(),
			new Str(),
		}.OrderByDescending(op => op.Operation.Length).ToList();

		var handler = operations.FirstOrDefault(op => operation.ToLower().StartsWith(op.Operation));
		if (handler != null) {
			handler.Assemble(line, tokenQueue, code);
			return;
		}

		foreach (var opHandler in _operationMap) {
			if (operation.ToLower().StartsWith(opHandler.Key)) {
				opHandler.Value(line, tokenQueue, code);
				return;
			}
		}
		throw new NotImplementedException($"No Handler found for Operation '{operation}'. Line: '{line}'");
	}

	private delegate void AddOperations(string line, TokenQueue tokens, ARMMachineCode code);

	private Dictionary<string, AddOperations> _operationMap = new() {
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

		// Multiplication
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
				Condition = condition,
				Opcode = MULOperation.MUL,
				DestinationRegister = destinationRegister,
				Op1Register = op1,
				Op2Register = op2,
			});
		}},
		{ "umull", (string line, TokenQueue tokens, ARMMachineCode code) => {
			tokens.Operation.DequeueValue("umull");
			var condition = tokens.Operation.DequeueCondition();
			tokens.Operation.AssertEmpty();

			var rdLo = tokens.DequeueRegister();
			tokens.DequeueComma();
			var rdHi = tokens.DequeueRegister();
			tokens.DequeueComma();
			var rm = tokens.DequeueRegister();
			tokens.DequeueComma();
			var rs = tokens.DequeueRegister();
			tokens.AssertEmpty();
			code.Add(new Multiply {
				Condition = condition,
				Opcode = MULOperation.UMULL,
				DestinationRegister = rdHi,
				AccumulateRegister = rdLo,
				Op1Register = rm,
				Op2Register = rs,
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

		// Test
		{ "cmp", (string line, TokenQueue tokens, ARMMachineCode code) => {
			LoadTestOperation(line, tokens, code, ALUOperation.CMP);
		}},
		{ "cmn", (string line, TokenQueue tokens, ARMMachineCode code) => {
			LoadTestOperation(line, tokens, code, ALUOperation.CMN);
		}},
		{ "teq", (string line, TokenQueue tokens, ARMMachineCode code) => {
			LoadTestOperation(line, tokens, code, ALUOperation.TEQ);
		}},
		{ "tst", (string line, TokenQueue tokens, ARMMachineCode code) => {
			LoadTestOperation(line, tokens, code, ALUOperation.TST);
		}},

		{ "mov", (string line, TokenQueue tokens, ARMMachineCode code) => {
			tokens.Operation.DequeueValue("mov");
			var condition = tokens.Operation.DequeueCondition();
			var setConditionCodes = tokens.Operation.DequeueFlagIfPresent("s");
			tokens.Operation.AssertEmpty();

			var destinationRegister = tokens.DequeueRegister();
			tokens.DequeueComma();
			var op2 = tokens.DequeueAluOp2();
			tokens.AssertEmpty();
			code.Add(new DataProcessing {
				Condition = condition,
				Operation = ALUOperation.MOV,
				DestinationRegister = destinationRegister,
				Op2 = op2,
				SetConditionCodes = setConditionCodes
			});
		}},
		{ "mvn", (string line, TokenQueue tokens, ARMMachineCode code) => {
			tokens.Operation.DequeueValue("mvn");
			var setConditionCodes = tokens.Operation.DequeueFlagIfPresent("s");
			var condition = tokens.Operation.DequeueCondition();
			tokens.Operation.AssertEmpty();

			var destinationRegister = tokens.DequeueRegister();
			tokens.DequeueComma();
			var op2 = tokens.DequeueAluOp2();
			tokens.AssertEmpty();
			code.Add(new DataProcessing {
				Condition = condition,
				Operation = ALUOperation.MVN,
				DestinationRegister = destinationRegister,
				Op2 = op2,
				SetConditionCodes = setConditionCodes
			});
		}},
		{ "lsl", (string line, TokenQueue tokens, ARMMachineCode code) => {
			LoadLogicalShiftOperation(line, tokens, code, ShiftType.LSL);
		}},
		{ "lsr", (string line, TokenQueue tokens, ARMMachineCode code) => {
			LoadLogicalShiftOperation(line, tokens, code, ShiftType.LSR);
		}},
		{ "rrx", (string line, TokenQueue tokens, ARMMachineCode code) => {
			tokens.Operation.DequeueValue("rrx");
			var setConditionCodes = tokens.Operation.DequeueFlagIfPresent("s");
			var condition = tokens.Operation.DequeueCondition();
			tokens.Operation.AssertEmpty();

			var destinationRegister = tokens.DequeueRegister();
			tokens.DequeueComma();
			var op2Register = tokens.DequeueRegister();
			var op2 = new ARM.ALU.Register(op2Register) {
				ShiftType = ShiftType.ROR,
				ShiftByRegister = false,
				ShiftAmount = 0
			};
			tokens.AssertEmpty();
			code.Add(new DataProcessing {
				Condition = condition,
				Operation = ALUOperation.MOV,
				DestinationRegister = destinationRegister,
				Op2 = op2,
				SetConditionCodes = setConditionCodes
			});
		}},

		// branch
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
	};

	public static void LoadLogicalShiftOperation(string line, TokenQueue tokens, ARMMachineCode code, ShiftType shiftType) {
		tokens.Operation.DequeueValue(shiftType.ToString());
		var setConditionCodes = tokens.Operation.DequeueFlagIfPresent("s");
		var condition = tokens.Operation.DequeueCondition();
		tokens.Operation.AssertEmpty();

		var destinationRegister = tokens.DequeueRegister();
		tokens.DequeueComma();
		var op2Register = tokens.DequeueRegister();
		var op2 = new ARM.ALU.Register(op2Register) {
			ShiftType = shiftType
		};

		tokens.DequeueComma();
		var next = tokens.Dequeue();
		if (next == "#") {
			var immediate = tokens.DequeueImmediate();
			op2.ShiftByRegister = false;
			op2.ShiftAmount = (byte)immediate;
		} else {
			var shiftRegister = tokens.ParseRegister(next);
			op2.ShiftByRegister = true;
			op2.ShiftRegister = shiftRegister;
		}
		tokens.AssertEmpty();
		code.Add(new DataProcessing {
			Condition = condition,
			Operation = ALUOperation.MOV,
			DestinationRegister = destinationRegister,
			SetConditionCodes = setConditionCodes,
			Op2 = op2
		});
	}
	
	public static void LoadTestOperation(string line, TokenQueue tokens, ARMMachineCode code, ALUOperation operation) {
		tokens.Operation.DequeueValue(operation.ToString());
		var condition = tokens.Operation.DequeueCondition();
		tokens.Operation.AssertEmpty();

		var op1 = tokens.DequeueRegister();
		tokens.DequeueComma();
		var op2 = tokens.DequeueAluOp2();
		tokens.AssertEmpty();
		code.Add(new DataProcessing {
			Operation = operation,
			SetConditionCodes = true,
			Op1Register = op1,
			Op2 = op2,
			Condition = condition
		});
	}

	public static void LoadALUOperation(string line, TokenQueue tokens, ARMMachineCode code, ALUOperation operation) {
		tokens.Operation.DequeueValue(operation.ToString());
		var setConditionCodes = tokens.Operation.DequeueFlagIfPresent("s");
		var condition = tokens.Operation.DequeueCondition();
		tokens.Operation.AssertEmpty();

		var destinationRegister = tokens.DequeueRegister();
		tokens.DequeueComma();
		var op1 = tokens.DequeueRegister();
		tokens.DequeueComma();
		var op2 = tokens.DequeueAluOp2();
		tokens.AssertEmpty();
		code.Add(new DataProcessing {
			Condition = condition,
			Operation = operation,
			DestinationRegister = destinationRegister,
			Op1Register = op1,
			Op2 = op2,
			SetConditionCodes = setConditionCodes
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
		if (operation == "push" || next == "d") {
			upDown = UpDown.Down;
		} else if (operation == "pop" || next == "i" || next == null) {
			upDown = UpDown.Up;
		} else {
			throw new NotSupportedException($"Could not interpret Up/Down bit for Block Data Transfer command. Line '{line}'");
		}

		next = tokens.Operation.TryDequeue(1, out var pp) ? pp: null;
		PrePost prePost;
		if (operation == "push" || next == "b") {
			prePost = PrePost.Pre;
		} else if (operation == "pop" || next == "a" || next == null) {
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
