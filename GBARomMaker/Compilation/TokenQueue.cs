using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using GBARomMaker.ARM.ALU;

public class TokenQueue : IEnumerable<string> {
	private string[] _tokens;
	private Queue<string> _tokenQueue;
	private string _line;

	public TokenQueue(string[] tokens, string line) {
		_tokens = tokens;
		_tokenQueue = new Queue<string>(tokens);
		_line = line;
	}

	public string Operation => _tokens[0];
	public string Dequeue() => _tokenQueue.Dequeue();

	public ALUOp2 DequeueAluOp2() {
		var next = _tokenQueue.Dequeue();
		if (next == "#") {
			var immediate = DequeueImmediate();
			return new Immediate(immediate);
		} else {
			var op2Register = DequeueRegister();
			return new Register(op2Register);
		}
	}

	public byte DequeueRegister() {
		var register = Dequeue();
		return register switch {
			"sp" => 13,
			"lr" => 14,
			"pc" => 15,
			_ => byte.TryParse(register.Substring(1), out var r) ? r : throw new Exception($"Failed to parse {register} as a register")
		};
	}

	public uint DequeueImmediate() {
		var immediate = Dequeue();
		return (uint)(immediate.StartsWith("0x", StringComparison.OrdinalIgnoreCase)
			? Convert.ToInt32(immediate[2..], 16)
			: Convert.ToInt32(immediate, 10));
	}

	public void AssertOperationLength(int length) {
		if (Operation.Length != length) throw new Exception($"Unexpected opcode '{Operation}'. Line '{_line}'");
	}

	public void AssertEmpty() {
		if (_tokenQueue.Any()) throw new Exception($"Too many arguments for '{Operation}'. Line '{_line}'");
	}
	
	public void DequeueComma() {
		var seperator = Dequeue();
		if (seperator != ",") throw new Exception($"Expected a comma between arguments. Line '{_line}'");
	}

    public IEnumerator<string> GetEnumerator() => _tokenQueue.GetEnumerator();
    IEnumerator IEnumerable.GetEnumerator() => _tokenQueue.GetEnumerator();
}
