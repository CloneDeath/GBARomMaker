using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using System.Reflection.Metadata;
using GBARomMaker.CIL;
using GBARomMaker.CILParse;

namespace GBARomMaker.CILToArm.ControlFlow;

public class ControlFlowGraph {
	public IReadOnlyList<InstructionMetadata> Instructions => _instructions.ToArray();
	private List<InstructionMetadata> _instructions = new();

	private readonly CILFactory _factory;

	public ControlFlowGraph(CILInstruction[] instructions, CILFactory factory, ICILMethod method) {
		_factory = factory;

		// Populate Instructions & Lookup
		var offset = 0;
		foreach (var instruction in instructions) {
			var metadata = new InstructionMetadata(offset, instruction, _factory, method);
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
		_instructions[0].StackTypes = new SignatureTypeCode[0];
		var toPopulate = new Queue<InstructionMetadata>();
		foreach (var next in _instructions[0].Next) {
			toPopulate.Enqueue(next);
		}
		while (toPopulate.Any()) {
			var current = toPopulate.Dequeue();
			if (current.StackTypes != null) continue; // already populated
			current.StackTypes = current.Previous.First(i => i.StackTypes != null).NextStackTypes;
			foreach (var next in current.Next) {
				toPopulate.Enqueue(next);
			}
		}
	}

	public void Print() {
		foreach (var instruction in Instructions) {
			Console.WriteLine($"{instruction.Offset:D4}: {instruction.GetCIL()}");
		}
	}
}
