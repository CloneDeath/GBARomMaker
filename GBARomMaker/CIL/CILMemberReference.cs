using System;
using System.Reflection.Metadata;
using System.Reflection.PortableExecutable;
using GBARomMaker.CIL.Blobs;

namespace GBARomMaker.CIL;

public class CILMemberReference : ICILMethod {
	private readonly PEReader _peReader;
	private readonly MetadataReader _metadata;
	private readonly MemberReference _self;
	private readonly MethodSignatureBlob _signature;
	
	public CILMemberReference(PEReader peReader, MetadataReader metadata, MemberReference self) {
		this._peReader = peReader;
		this._metadata = metadata;
		this._self = self;
		this._signature = new MethodSignatureBlob(metadata, self.Signature);
	}
    
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
	public byte[] BodyBytes => throw new NotImplementedException($"For {FullName}");
	public bool IsInstance => _signature.IsInstance;
    public int ParameterCount => _signature.ParameterCount;
	public ISignatureType ReturnType => _signature.ReturnType;
	public bool HasReturnValue => ReturnType.Code != SignatureTypeCode.Void;
	public ISignatureType[] GetArgumentTypes() => _signature.ArgumentTypes;

	public MemberReferenceKind Kind => _self.GetKind();
    public bool IsNativeInvoke => throw new NotImplementedException($"For {FullName}");
    public string NativeInvokeTarget => throw new NotImplementedException($"For {FullName}");
    public ISignatureType[] GetLocalVariableTypes() => throw new NotImplementedException($"For {FullName}");
}
