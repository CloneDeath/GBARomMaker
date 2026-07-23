using System;
using System.Linq;
using System.Reflection.Metadata;
using System.Reflection.PortableExecutable;

namespace GBARomMaker.CIL;

public class CILMethodDefinition : ICILMethod {
	private readonly PEReader _peReader;
	private readonly MetadataReader _metadata;
	private readonly MethodDefinition _method;
	
	public CILTypeDefinition Class => new(_peReader, _metadata, _metadata.GetTypeDefinition(_method.GetDeclaringType()));

	public CILMethodDefinition(PEReader peReader, MetadataReader metadata, MethodDefinition method) {
		this._peReader = peReader;
		this._metadata = metadata;
		this._method = method;
	}

	public string Name => _metadata.GetString(_method.Name);
	public string FullName => $"{Class.Namespace}.{Class.Name}.{Name}";

	public byte[] BodyBytes => _peReader.GetMethodBody(_method.RelativeVirtualAddress)?.GetILBytes() ?? [];

	public int ArgumentCount {
		get {
			var parameters = _method.GetParameters().Select(p => _metadata.GetParameter(p));
			return parameters.Where(p => p.SequenceNumber > 0).Count();
		}
	}

	public bool IsInstanceMethod {
		get {
			return _method.GetParameters().Select(p => _metadata.GetParameter(p)).Any(p => p.SequenceNumber == 0);
		}
	}
}
