using System;
using System.Reflection.Metadata;
using CILBoy.CIL.Blobs;

namespace CILBoy.CIL;

public class CILMemberReference : ICILMethod {
	private CILAssemblyFactory _factory;
	private readonly MemberReference _self;
	private readonly MethodSignatureBlob _signature;
	
	public CILMemberReference(CILAssemblyFactory factory, MemberReference self) {
		this._factory = factory;
		this._self = self;
		this._signature = factory.GetMethodSignatureBlob(self.Signature);
	}
    
	public ICILType Parent => _factory.GetTypeDefinition(_self.Parent);
	public string Name => _factory.GetString(_self.Name);
	public string FullName => $"{Parent.Namespace}.{Parent.Name}.{Name}";
	public byte[] BodyBytes => throw new NotImplementedException($"Reference For {FullName}");
	public bool IsInstance => _signature.IsInstance;
    public int ParameterCount => _signature.ParameterCount;
	public ISignatureType ReturnType => _signature.ReturnType;
	public bool HasReturnValue => ReturnType.Code != SignatureTypeCode.Void;
	public ISignatureType[] GetArgumentTypes() => _signature.ArgumentTypes;

	public MemberReferenceKind Kind => _self.GetKind();
    public bool IsNativeInvoke {
		get {
			var definingFactory = _factory.GetAssemblyFactoryFor(((CILTypeReference)Parent).Assembly.Name);
			var parent = definingFactory.GetTypeDefinition(Parent.FullName);
			var method = parent.GetMethodDefinition(Name);
			return method.IsNativeInvoke;
		}
	}
    public string NativeInvokeTarget => throw new NotImplementedException($"Reference For {FullName}");
    public ISignatureType[] GetLocalVariableTypes() => throw new NotImplementedException($"Reference For {FullName}");

	public override string ToString() {
		var arguments = GetArgumentTypes();
		var argsString = string.Join(", ", arguments);
		return $"{ReturnType} {FullName}({argsString})";
	}
}
