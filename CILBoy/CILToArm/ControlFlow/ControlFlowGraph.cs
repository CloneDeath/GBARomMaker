using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using CILBoy.CIL;
using CILBoy.CILParse;

namespace CILBoy.CILToArm.ControlFlow;

public class ControlFlowGraph {
	public IReadOnlyList<InstructionMetadata> Instructions => _instructions.ToArray();
	private List<InstructionMetadata> _instructions = new();

	public ControlFlowGraph(CILInstruction[] instructions, ICILMethod method) {
		// Populate Instructions & Lookup
		var offset = 0;
		foreach (var instruction in instructions) {
			var metadata = new InstructionMetadata(offset, instruction, method);
			_instructions.Add(metadata);
			offset += instruction.GetBytes().Length;
		}

		// Connect Branches
		for (var i = 0; i < _instructions.Count - 1; i++) {
			var instruction = _instructions[i];
			if (instruction.OpCode == OpCodes.Ret) continue;

			// Skip Always branches, they never go into next
			if (!instruction.AlwaysBranches) {
				var next = _instructions[i+1];
				instruction.AddNext(next);
			}

			if (instruction.SometimesBranches || instruction.AlwaysBranches) {
				var targetOffset = instruction.Offset + instruction.Length + instruction.BranchTarget;
				var target = _instructions.First(i => i.Offset == targetOffset);
				instruction.AddNext(target);
			}
		}

		// Populate Stack
		_instructions[0].StackTypes = new Stack<ISignatureType>();
		var toPopulate = new Queue<InstructionMetadata>(_instructions.ToArray());
		while (toPopulate.Any()) {
			var current = toPopulate.Dequeue();
			if (current.StackTypes != null) continue; // already populated
			if (!current.Previous.Any()) { // unreachable
				current.StackTypes = [];
				continue;
			};
			var previousPopulated = current.Previous.FirstOrDefault(i => i.StackTypes != null);
			if (previousPopulated == null) {
				// push the previouses onto the queue to be done first, then revisit this one
				foreach (var prev in current.Previous) {
					toPopulate.Enqueue(prev);
				}
				toPopulate.Enqueue(current);
				continue;
			}
			current.StackTypes = previousPopulated.NextStackTypes;
		}
	}

	public void Print() {
		foreach (var instruction in Instructions) {
			Console.WriteLine($"{instruction.Offset:D4}: {instruction.GetCIL()}");
		}
	}
}
