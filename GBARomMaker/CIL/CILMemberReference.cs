using System;
using System.Reflection.Metadata;
using System.Reflection.PortableExecutable;

namespace GBARomMaker.CIL;

public class CILMemberReference : ICILMethod {
	private readonly PEReader _peReader;
	private readonly MetadataReader _metadata;
	private readonly MemberReference _self;
	
	public CILMemberReference(PEReader peReader, MetadataReader metadata, MemberReference self) {
		this._peReader = peReader;
		this._metadata = metadata;
		this._self = self;
	}


	public MemberReferenceKind Kind => _self.GetKind();

    public ICILType Parent {
        get {
			var factory = new CILFactory(_peReader, _metadata);
			switch (_self.Parent.Kind) {
				case HandleKind.TypeDefinition: {
					return factory.GetTypeDefinition((TypeDefinitionHandle)_self.Parent);
				}
				case HandleKind.TypeReference: {
					return factory.GetTypeReference((TypeReferenceHandle)_self.Parent);
				}
				default: {
					throw new NotImplementedException($"Unexpected parent handle kind: {_self.Parent.Kind}");
				}
			}
        }
    }

	public string Name => _metadata.GetString(_self.Name);
	public string FullName => $"{Parent.Namespace}.{Parent.Name}.{Name}";
	public byte[] BodyBytes => throw new NotImplementedException();
    public bool IsInstanceMethod => throw new NotImplementedException();
    public int ArgumentCount => throw new NotImplementedException();
}
