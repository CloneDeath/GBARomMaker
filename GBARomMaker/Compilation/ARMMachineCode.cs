using GBARomMaker.ARM;
using System.Collections.Generic;
using System.Linq;

namespace GBARomMaker.Compilation;

public record ARMLine(int Offset, IInstruction Instruction);
public record NeedsLabel(int Offset, ILabeledInstruction Instruction, string Label);
public record LabelLocation(string Label, int Offset);

public class ARMMachineCode : List<ARMLine> {
	private List<LabelLocation> _labels = new();
	private List<NeedsLabel> _needsLabels = new();

	private int offset = 0;

	public void Add(params IInstruction[] instructions) {
		foreach (var instruction in instructions) {		
			var line = new ARMLine(offset, instruction);
			Add(line);
			offset += instruction.ToBytes().Length;
		}
	}

	public void AddNeedsLabel(ILabeledInstruction instruction, string label) {
		_needsLabels.Add(new NeedsLabel(offset, instruction, label));
		Add(instruction);
	}

	public void AddLabel(string label) {
		_labels.Add(new LabelLocation(label, offset));

		var needs = _needsLabels.Where(n => n.Label == label).ToList();
		_needsLabels.RemoveAll(n => n.Label == label);

		foreach (var need in needs) {
			var delta = offset - need.Offset;
			need.Instruction.SetOffset(delta);
		}
	}

    public byte[] ToBytes() {
		return this.SelectMany(l => l.Instruction.ToBytes()).ToArray();
    }
}
