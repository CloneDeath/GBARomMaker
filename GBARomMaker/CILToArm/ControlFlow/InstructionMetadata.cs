using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using GBARomMaker.CIL;
using GBARomMaker.CILParse;

namespace GBARomMaker.CILToArm.ControlFlow;

public class InstructionMetadata {
	public readonly int Offset;
    public CILInstruction Instruction { get; }
	public Stack<ISignatureType>? StackTypes { get; set; }
	public List<InstructionMetadata> Next { get; } = new List<InstructionMetadata>();
	public List<InstructionMetadata> Previous { get; } = new List<InstructionMetadata>();
    
	private readonly CILFactory _factory;
    private readonly ICILMethod _method;

    public InstructionMetadata(int offset, CILInstruction instruction, CILFactory factory, ICILMethod method) {
		this.Offset = offset;
		this.Instruction = instruction;
        this._factory = factory;
        this._method = method;
    }

	public OpCode OpCode => Instruction.OpCode;

	public bool AlwaysBranches => Instruction.AlwaysBranches;
	public bool SometimesBranches => Instruction.SometimesBranches;
	public int? BranchTarget => Instruction.BranchTarget;
    public byte[] GetBytes() => Instruction.GetBytes();

    public void AddNext(InstructionMetadata next) {
		if (!Next.Contains(next)) Next.Add(next);
		if (!next.Previous.Contains(this)) next.Previous.Add(this);
    }

    public string GetCIL() => Instruction.GetCIL(_factory, _method);

    public int Length => GetBytes().Length;

	public Stack<ISignatureType> NextStackTypes {
		get {
			if (StackTypes == null) throw new System.InvalidOperationException("Stack wasn't set");
			var stack = new Stack<ISignatureType>(StackTypes.Reverse());
			Instruction.ModifyStack(_factory, _method, stack);
			return stack;
		}
	}

	public override string ToString() {
		var stack = StackTypes == null ? "null" : string.Join(", ", StackTypes ?? []);
		return $"{nameof(InstructionMetadata)} {{ '{GetCIL()}', offset: { Offset }, stack: [{ stack }] }}";
	}
}
