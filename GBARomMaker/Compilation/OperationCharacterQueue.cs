using System;
using System.Collections.Generic;
using GBARomMaker.ARM.Common;

namespace GBARomMaker.Compilation;

public class OperationCharacterQueue {
	private string _operation;
	private int _index = 0;
	private readonly string _line;

	public OperationCharacterQueue(string operation, string line) {
		_operation = operation;
		_line = line;
	}

	public void DequeueValue(string value) {
		var current = _operation[_index..(_index + value.Length)];
		if (current.ToLower() != value.ToLower()) throw new Exception($"Expected '{value}', but found '{current}'. Line '{_line}'");

		_index += value.Length;
	}

	public int Index => _index;

	private static readonly Dictionary<string, Condition> Conditions = new() {
		["EQ"] = Condition.EQ,
		["NE"] = Condition.NE,
		
		["CS"] = Condition.CS,
		["HS"] = Condition.CS,
		
		["CC"] = Condition.CC,
		["LO"] = Condition.CC,
		
		["MI"] = Condition.MI,
		["PL"] = Condition.PL,
		["VS"] = Condition.VS,
		["VC"] = Condition.VC,
		["HI"] = Condition.HI,
		["LS"] = Condition.LS,
		["GE"] = Condition.GE,
		["LT"] = Condition.LT,
		["GT"] = Condition.GT,
		["LE"] = Condition.LE,
		["AL"] = Condition.AL,
		["NV"] = Condition.NV,
	};

	public bool DequeueFlagIfPresent(string flag) {
		if (_index + flag.Length > _operation.Length) {
			return false;
		}

		var next = _operation[_index .. (_index + flag.Length)];
		if (flag == next) {
			_index += flag.Length;
			return true;
		}

		return false;
	}

	public Condition DequeueCondition() {
		if (_index + 2 > _operation.Length) {
			return Condition.Always;
		}

		var possibleCondition = _operation[_index .. (_index + 2)];
		if (Conditions.ContainsKey(possibleCondition.ToUpper())) {
			_index += 2;
			return Conditions[possibleCondition.ToUpper()];
		}

		return Condition.Always;
	}

	public void AssertEmpty() {
		if (_index != _operation.Length) {
			throw new Exception($"Unexpected flags for operation '{_operation}'. Line '{_line}'");
		}
	}

	public bool TryDequeue(int length, out string value) {
		if (_index + length > _operation.Length) {
			value = "";
			return false;
		}

		value = _operation[_index .. (_index + length)];
		_index += length;
		return true;
	}
}
