using System;
using GBARomMaker.ARM.ALU;

namespace GBARomMaker.ARM;

public class DataProcessingWithLabelOffset : DataProcessing, ILabeledInstruction {
    public void SetOffset(int offset) {
		var adjusted = offset - 8;
		Operation = adjusted < 0 ? ALUOperation.SUB : ALUOperation.ADD;
		Op2 = new Immediate((uint)Math.Abs(adjusted));
    }
}
