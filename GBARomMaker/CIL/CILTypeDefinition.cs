using System;
using System.Linq;
using System.Reflection.Metadata;
using System.Reflection.PortableExecutable;

namespace GBARomMaker.CIL;

public class CILTypeDefinition : ICILType {
	private readonly PEReader _peReader;
	private readonly MetadataReader _metadata;
	private readonly TypeDefinition _self;
	
	public CILTypeDefinition(PEReader peReader, MetadataReader metadata, TypeDefinition self) {
		this._peReader = peReader;
		this._metadata = metadata;
		this._self = self;
	}

	public string Namespace => _metadata.GetString(_self.Namespace);
	public string Name => _metadata.GetString(_self.Name);
	public string FullName => $"{Namespace}.{Name}";

	public int FieldCount {
		get {
			var fields = _self.GetFields().Select(f => _metadata.GetFieldDefinition(f));
			return fields.Count();
		}
	}

	public CILMethodDefinition GetMethodDefinition(string name) {
		var methods = _self.GetMethods().Select(m => _metadata.GetMethodDefinition(m));
		var method = methods.First(m => _metadata.GetString(m.Name) == name);
		return new CILMethodDefinition(_peReader, _metadata, method);		
	}
}
