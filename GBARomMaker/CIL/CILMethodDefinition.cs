using System;
using System.Linq;
using System.Reflection.Metadata;
using System.Reflection.PortableExecutable;

namespace GBARomMaker.CIL;

public class CILMethodDefinition : ICILMethod {
	private readonly PEReader _peReader;
	private readonly MetadataReader _metadata;
	private readonly MethodDefinition _method;
	
	public CILTypeDefinition Parent => new(_peReader, _metadata, _metadata.GetTypeDefinition(_method.GetDeclaringType()));

	public CILMethodDefinition(PEReader peReader, MetadataReader metadata, MethodDefinition method) {
		this._peReader = peReader;
		this._metadata = metadata;
		this._method = method;
	}

	public string Name => _metadata.GetString(_method.Name);
	public string FullName => $"{Parent.Namespace}.{Parent.Name}.{Name}";

	public byte[] BodyBytes => _peReader.GetMethodBody(_method.RelativeVirtualAddress)?.GetILBytes() ?? [];

	public int ParameterCount => _method.GetParameters().Count();

	public bool IsInstance {
		get {
			var signature = _metadata.GetBlobReader(_method.Signature);
			var header = signature.ReadSignatureHeader();
			return header.IsInstance;
		}
	}

	public bool IsConstructor => Name == ".ctor" || Name == ".cctor";
}
