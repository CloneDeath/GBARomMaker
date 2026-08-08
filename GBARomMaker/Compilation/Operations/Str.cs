using System;
using GBARomMaker.ARM;
using GBARomMaker.ARM.Common;

namespace GBARomMaker.Compilation.Operations;

public class Str : IOperationAssembler {
	public string Operation => "str";
	public void Assemble(string line, TokenQueue tokens, ARMMachineCode code) {
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
	}
}
