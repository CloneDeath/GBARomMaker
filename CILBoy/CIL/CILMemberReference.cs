using System.Reflection.Metadata;
using CILBoy.CIL.Blobs;

namespace CILBoy.CIL;

public class CILMemberReference : ICILMethod {
	private readonly MemberReference _self;
	private readonly MethodSignatureBlob _signature;

	public CILAssemblyFactory Factory { get; }
	
	public CILMemberReference(CILAssemblyFactory factory, MemberReference self) {
		this.Factory = factory;
		this._self = self;
		this._signature = factory.GetMethodSignatureBlob(self.Signature);
	}
    
	public ICILType Parent => Factory.GetTypeDefinition(_self.Parent);
	public string Name => Factory.GetString(_self.Name);
	public string FullName => $"{Parent.Namespace}.{Parent.Name}.{Name}";
	public bool IsInstance => _signature.IsInstance;
    public int ParameterCount => _signature.ParameterCount;
	public ISignatureType ReturnType => _signature.ReturnType;
	public bool HasReturnValue => ReturnType.Code != SignatureTypeCode.Void;
	public ISignatureType[] GetArgumentTypes() => _signature.ArgumentTypes;

	public MemberReferenceKind Kind => _self.GetKind();

	public override string ToString() {
		var arguments = GetArgumentTypes();
		var argsString = string.Join(", ", arguments);
		return $"<ref> {ReturnType} {FullName}({argsString})";
	}

	public CILMethodDefinition GetMethodDefinition() => Parent.GetMethodDefinition(Name);
}
