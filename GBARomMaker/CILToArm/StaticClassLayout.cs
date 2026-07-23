using System;
using GBARomMaker.CIL;

namespace GBARomMaker.CILToArm;

public class StaticClassLayout {
	public int StartAddress;

	private CILTypeDefinition _type;
	private CILFieldDefinition[] _fields;

	public StaticClassLayout(CILTypeDefinition type, int startAddress) {
		_type = type;
		StartAddress = startAddress;
		_fields = type.Fields;
	}

	public string FullName => _type.FullName;
	public int Size => _type.FieldCount * 4;

	public int GetFieldOffset(CILFieldDefinition field) {
		for (var i = 0; i < _fields.Length; i++) {
			var candidate = _fields[i];
			if (candidate.Name != field.Name) continue;

			return i * 4;
		}
		throw new Exception($"Field '{field.Name}' does not exist in type '{FullName}'");
	}
}
