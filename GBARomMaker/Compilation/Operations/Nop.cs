using GBARomMaker.ARM;
using GBARomMaker.ARM.ALU;

namespace GBARomMaker.Compilation.Operations;

public class Nop : IOperationAssembler {
	public string Operation => "nop";
	public void Assemble(string line, TokenQueue tokens, ARMMachineCode code) {
		tokens.Operation.DequeueValue("nop");
		tokens.Operation.AssertEmpty();
		tokens.AssertEmpty();
		code.Add(new DataProcessing {
			Operation = ALUOperation.MOV,
			DestinationRegister = 0,
			Op2 = new Register(0)
		});
	}
}
