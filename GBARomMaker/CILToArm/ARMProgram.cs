using System.Collections.Generic;
using System.Linq;

namespace GBARomMaker.CILToArm;

public class ARMProgram : List<ARMLine> {
	public int Offset { get; private set; } = 0;

	public void Add(int size, params string[] lines) {
		var existing = this.Where(l => l.CilOffset == Offset);
		var maxOrder = existing.Any() ? existing.Max(l => l.Order) + 1 : 0;
		for (var i = 0; i < lines.Length; i++) {
			this.Add(new ARMLine(Offset, maxOrder + i, lines[i]));
		}
		Offset += size;
	}

	public void AddLabel(int target, string line) {
		var existing = this.Where(l => l.CilOffset == Offset);
		var minOrder = existing.Any() ? existing.Min(l => l.Order) + 1 : 0;
		this.Add(new ARMLine(target, minOrder, line));
	}

	public string[] GetArm7Assembly() {
		return this.OrderBy(l => l.Order).OrderBy(l => l.CilOffset).Select(l => l.Instruction).ToArray();
	}
}
