using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using CILBoy.ARM.ALU;
using CILBoy.ARM.Common;

namespace CILBoy.Compilation;

public class TokenQueue : IEnumerable<string> {
	private string[] _tokens;
	private Queue<string> _tokenQueue;
	private string _line;

	public TokenQueue(string[] tokens, string line) {
		_tokens = tokens;
		_tokenQueue = new Queue<string>(tokens);
		_line = line;
		Operation = new(_tokens[0], line);
	}

	public OperationCharacterQueue Operation { get; private set; }
	public string Dequeue() => _tokenQueue.Dequeue();

	public ALUOp2 DequeueAluOp2() {
		var next = _tokenQueue.Dequeue();
		if (next == "#") {
			var immediate = DequeueImmediate();
			return new Immediate(immediate);
		} else {
			var op2Register = ParseRegister(next);
			if (!_tokenQueue.Any()) return new Register(op2Register);
			DequeueComma();

			if (Peek() == "rrx") {
				Dequeue();
				DequeueToken("#");
				var amount = DequeueImmediate();
				if (amount != 1) throw new Exception($"rrx is only valid with immediate value 1, got {amount}. Line '{_line}'");

				// RRX#1 is encoded as an ROR#0
				return new Register(op2Register) {
					ShiftAmount = 0,
					ShiftType = ShiftType.ROR,
					ShiftByRegister = false
				};
			}

			var shiftType = DequeueShiftType();

			next = _tokenQueue.Dequeue();
			if (next == "#") {
				var immediate = DequeueImmediate();
				return new Register(op2Register) {
					ShiftAmount = (byte)immediate,
					ShiftType = shiftType,
					ShiftByRegister = false
				};
			}

			var shiftRegister = ParseRegister(next);
			return new Register(op2Register) {
				ShiftRegister = shiftRegister,
				ShiftType = shiftType,
				ShiftByRegister = true
			};
		}
	}

	public byte DequeueRegister() {
		return ParseRegister(Dequeue());
	}

	public byte ParseRegister(string register) {
		if (register.StartsWith("r") && byte.TryParse(register.Substring(1), out var r) && r >= 0 && r <= 15) {
			return r;
		}

		if (register.StartsWith("a") && byte.TryParse(register.Substring(1), out var a) && a >= 1 && a <= 4) {
			return (byte)(a - 1);
		}
		
		if (register.StartsWith("v") && byte.TryParse(register.Substring(1), out var v) && v >= 1 && v <= 8) {
			return (byte)(v + 3);
		}

		return register switch {
			"fp" => 11,
			"ip" => 12,
			"sp" => 13,
			"lr" => 14,
			"pc" => 15,
			_ => throw new Exception($"Failed to parse {register} as a register. Line '{_line}'")
		};
	}

	public int DequeueSignedImmediate() {
		var immediate = Dequeue();
		var negate = false;
		if (immediate == "-") {
			immediate = Dequeue();
			negate = true;
		}
		var value = (immediate.StartsWith("0x", StringComparison.OrdinalIgnoreCase)
			? Convert.ToInt32(immediate[2..], 16)
			: Convert.ToInt32(immediate, 10));
		return negate ? -value : value;
	}

	public uint DequeueImmediate() {
		var immediate = Dequeue();
		try {
			return (uint)(immediate.StartsWith("0x", StringComparison.OrdinalIgnoreCase)
				? Convert.ToInt32(immediate[2..], 16)
				: Convert.ToInt32(immediate, 10));
		}
		catch (FormatException ex) {
			throw new Exception($"Failed to parse number: {ex.Message} Line '{_line}'");
		}
	}

	public string Peek() => _tokenQueue.Peek();

	public void AssertEmpty() {
		if (_tokenQueue.Any()) throw new Exception($"Too many arguments for '{Operation}'. Peek: '{_tokenQueue.Peek()}'. Line '{_line}'");
	}
	
	public void DequeueComma() => DequeueToken(",");

	public void DequeueToken(string token) {
		var next = Dequeue();
		if (next != token) throw new Exception($"Expected a '{token}' between arguments, got '{next}'. Line '{_line}'");
	}

	public ShiftType DequeueShiftType() {
		var shiftType = Dequeue();
		return shiftType switch {
			"lsl" => ShiftType.LSL,
			"lsr" => ShiftType.LSR,
			"asr" => ShiftType.ASR,
			"ror" => ShiftType.ROR,
			_ => throw new Exception($"Expected a shift type, got '{shiftType}'. Line '{_line}'")
		};
	}


    public IEnumerator<string> GetEnumerator() => _tokenQueue.GetEnumerator();
    IEnumerator IEnumerable.GetEnumerator() => _tokenQueue.GetEnumerator();
}
