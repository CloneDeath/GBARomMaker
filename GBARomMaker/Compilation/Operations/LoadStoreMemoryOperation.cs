using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using GBARomMaker.ARM;
using GBARomMaker.ARM.ALU;
using GBARomMaker.ARM.Common;

namespace GBARomMaker.Compilation.Operations;

public class LoadStoreMemoryOperation(string operation) : IOperationAssembler {
	public string Operation => operation;

	private LoadStore LoadStoreType => operation switch {
		"ldr" => LoadStore.Load,
		"str" => LoadStore.Store,
		_ => throw new Exception($"Unexpected operation '{operation}'")
	};
	
	private HOpCode HOpCodeType => operation switch {
		"ldr" => HOpCode.LDRH,
		"str" => HOpCode.STRH,
		_ => throw new Exception($"Unexpected operation '{operation}'")
	};

	public void Assemble(string line, TokenQueue tokens, ARMMachineCode code) {
		tokens.Operation.DequeueValue(operation);
		var condition = tokens.Operation.DequeueCondition();
		var flag = tokens.Operation.TryDequeue(1, out var f) ? f : null;
		tokens.Operation.AssertEmpty();

		var sourceDestinationRegister = tokens.DequeueRegister();
		tokens.DequeueComma();

		var source = tokens.Dequeue();
		if (operation == "ldr" && source == "=") { // This is actual a psudocommand for MOV/ORs or PC Offset to Label
			LdrMovPseudoCommand(line, tokens, code, condition, sourceDestinationRegister, flag);
			return;
		}

		if (source != "[") throw new Exception($"Unexpected token when reading source: '{source}', expected '['. Line: '{line}'");

		var baseRegister = tokens.DequeueRegister();
		var next = tokens.Dequeue();
		if (next == "]") {
			tokens.AssertEmpty();
			code.Add(flag == "h"
				? new MemoryHalf {
					Condition = condition,
					BaseRegister = baseRegister,
					SourceDestinationRegister = sourceDestinationRegister,
					OpCode = HOpCodeType,
					ImmediateOffsetFlag = true,
					ImmediateOffset = 0,
					PrePost = PrePost.Pre,
					UpDown = UpDown.Up,
					WriteBack = false
				}
				: new SingleDataTransfer {
					Condition = condition,
					BaseRegister = baseRegister,
					SourceDestinationRegister = sourceDestinationRegister,
					LoadStore = LoadStoreType,
					Offset = new ARM.Memory.Immediate(0),
					PrePost = PrePost.Pre,
					UpDown = UpDown.Up,
					WriteBack = false,
					Word = flag == "b" ? false : true
				});
			return;
		}

		if (next != ",") throw new Exception($"Expected a comma between arguments, got '{next}'. Line '{line}'");

		next = tokens.Dequeue();
		UpDown updown = UpDown.Up;
		ARM.Memory.IOffset? offset = null;

		// for half-word transfers...
		bool immediateOffsetFlag = false;
		byte immediateOffset = 0;
		byte offsetRegister = 0;
		
		if (next == "#") {
			var immediate = tokens.DequeueSignedImmediate();
			updown = immediate < 0 ? UpDown.Down : UpDown.Up;
			offset = new ARM.Memory.Immediate((uint)Math.Abs(immediate));
			
			// half words...
			immediateOffsetFlag = true;
			immediateOffset = (byte)Math.Abs(immediate);
		} else {
			var register = tokens.ParseRegister(next);

			// half-words don't support shifted offsets
			if (flag != "h") {
				next = tokens.Peek();
				if (next == "]") {
					offset = new ARM.Memory.Register {
						OffsetRegister = register,
						ShiftAmount = 0,
						ShiftType = ShiftType.LSL
					};
				} else {
					tokens.DequeueComma();
					var shiftType = tokens.DequeueShiftType();
					tokens.DequeueToken("#");
					var shiftAmount = tokens.DequeueImmediate();
					offset = new ARM.Memory.Register {
						OffsetRegister = register,
						ShiftAmount = (byte)shiftAmount,
						ShiftType = shiftType
					};
				}
			}

			// half-words...
			immediateOffsetFlag = false;
			offsetRegister = register;
		}
		tokens.DequeueToken("]");
		tokens.AssertEmpty();

		code.Add(flag == "h"
			? new MemoryHalf {
				Condition = condition,
				BaseRegister = baseRegister,
				SourceDestinationRegister = sourceDestinationRegister,
				OpCode = HOpCodeType,
				ImmediateOffsetFlag = immediateOffsetFlag,
				ImmediateOffset = immediateOffset,
				OffsetRegister = offsetRegister,
				PrePost = PrePost.Pre,
				UpDown = updown,
				WriteBack = false
			}
			: new SingleDataTransfer {
				Condition = condition,
				BaseRegister = baseRegister,
				SourceDestinationRegister = sourceDestinationRegister,
				LoadStore = LoadStoreType,
				Offset = offset ?? throw new Exception("An offset wasn't provided!"),
				PrePost = PrePost.Pre,
				UpDown = updown,
				WriteBack = false,
				Word = flag == "b" ? false : true
			});
		return;
	}

	private void LdrMovPseudoCommand(string line, TokenQueue tokens, ARMMachineCode code, Condition condition, byte destinationRegister, string? flag) {
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
	}
}
