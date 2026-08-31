using System;
using System.Reflection.Metadata;
using System.Reflection.PortableExecutable;

namespace CILBoy.CIL;

public class CILTypeReference : ICILType {
	private readonly PEReader _peReader;
	private readonly MetadataReader _metadata;
	private readonly TypeReference _self;
	
	public CILTypeReference(PEReader peReader, MetadataReader metadata, TypeReference self) {
		this._peReader = peReader;
		this._metadata = metadata;
		this._self = self;
	}

	public string Namespace => _metadata.GetString(_self.Namespace);
	public string Name => _metadata.GetString(_self.Name);
	public string FullName => $"{Namespace}.{Name}";

	public CILAssemblyReference Assembly {
		get {
			var assembly = _metadata.GetAssemblyReference((AssemblyReferenceHandle)_self.ResolutionScope);
			return new CILAssemblyReference(assembly);
		}
	}

	public CILMethodDefinition GetMethodDefinition(string name) {
		throw new NotImplementedException($"{nameof(GetMethodDefinition)} not implemented for {FullName} -> {name}");
	}

	public override string ToString() => FullName;

	public CILFieldDefinition[] InstanceFields => throw new NotImplementedException();
	public CILFieldDefinition[] StaticFields => throw new NotImplementedException();

	public bool IsValueType => FullName == "System.ValueType";
	public bool IsEnum => FullName == "System.Enum";
}
