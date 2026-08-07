using System;
using GBARomMaker.CIL;

namespace GBARomMaker.CILToArm;

public class ClassLayout {
	private CILTypeDefinition _type;
	private CILFieldDefinition[] _fields;

	public ClassLayout(CILTypeDefinition type) {
		_type = type;
		_fields = type.InstanceFields;
	}

	public string FullName => _type.FullName;
	public int Size => _fields.Length * 4;

	public int GetFieldOffset(CILFieldDefinition field) {
		for (var i = 0; i < _fields.Length; i++) {
			var candidate = _fields[i];
			if (candidate.Name != field.Name) continue;

			return i * 4;
		}
		throw new Exception($"Field '{field.Name}' does not exist in type '{FullName}'");
	}
}
