using System.Collections.Generic;
using System.Linq;
using GBARomMaker.CIL;

namespace GBARomMaker.CILToArm;

public class ARMProgram : List<ARMLine> {
	private List<StaticClassLayout> _staticClasses = new();

	public int JumpCount = 0;
	public int Offset { get; private set; } = 0;

	public Queue<ICILMethod> MethodsToTranspile { get; } = new();
	public List<string> MethodsTranspiled { get; } = new();

	public int HeapStart = 0x02000000;

	public void Add(int size, params string[] lines) {
		var existing = this.Where(l => l.CilOffset == Offset);
		var maxOrder = existing.Any() ? existing.Max(l => l.Order) + 1 : 0;
		for (var i = 0; i < lines.Length; i++) {
			this.Add(new ARMLine(Offset, maxOrder + i, "\t" + lines[i]));
		}
		Offset += size;
	}

	public void AddLabel(int target, string label) {
		var existing = this.Where(l => l.CilOffset == target);
		var minOrder = existing.Any() ? existing.Min(l => l.Order) - 1 : 0;
		this.Add(new ARMLine(target, minOrder, $"{label}:"));
	}

	public void AddLabel(string label) {
		AddLabel(Offset, label);
	}

	public string[] GetArm7Assembly() {
		return this.OrderBy(l => l.Order).OrderBy(l => l.CilOffset).Select(l => l.Instruction).ToArray();
	}

	public StaticClassLayout GetStaticClassLayout(CILTypeDefinition type) {
		var existing = _staticClasses.FirstOrDefault(c => c.FullName == type.FullName);
		if (existing != null) return existing;

		var instance = new StaticClassLayout(type, HeapStart);
		HeapStart += instance.Size;
		_staticClasses.Add(instance);
		return instance;
	}
}
