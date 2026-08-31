using System.Reflection;
using System.Reflection.Metadata;
using CILBoy.CIL.Blobs;

namespace CILBoy.CIL;

public class CILMethodDefinition : ICILMethod {
	public CILAssemblyFactory Factory { get; }
    private readonly MethodDefinition _method;
	private readonly MethodSignatureBlob _signature;

	public CILMethodDefinition(CILAssemblyFactory factory, MethodDefinition method) {
        this.Factory = factory;
        this._method = method;
		this._signature = factory.GetMethodSignatureBlob(method.Signature);
	}

	public ICILType Parent => Factory.GetTypeDefinition(_method.GetDeclaringType());
	public string Name => Factory.GetString(_method.Name);
	public string FullName => $"{Parent.Namespace}.{Parent.Name}.{Name}";
	public byte[] BodyBytes => Factory.GetMethodBody(_method.RelativeVirtualAddress).GetILBytes() ?? [];
	public bool IsInstance => _signature.IsInstance;
	public int ParameterCount => _signature.ParameterCount;
	public ISignatureType ReturnType => _signature.ReturnType;
	public bool HasReturnValue => ReturnType.Code != SignatureTypeCode.Void;
	public ISignatureType[] GetArgumentTypes() => _signature.ArgumentTypes;

	public bool IsConstructor => IsInstanceConstructor || IsStaticConstructor;
	public bool IsInstanceConstructor => Name == ".ctor";
	public bool IsStaticConstructor => Name == ".cctor";

	public bool IsNativeInvoke {
		get {
			var isInvoke = _method.Attributes.HasFlag(MethodAttributes.PinvokeImpl);
			if (!isInvoke) return false;

			var import = _method.GetImport();
			var module = Factory.GetModuleReference(import.Module);
			var moduleName = Factory.GetString(module.Name);
			return moduleName == "gba";
		}
	}

	public string NativeInvokeTarget {
		get {
			var import = _method.GetImport();
			return Factory.GetString(import.Name);
		}
	}

	public ISignatureType[] GetLocalVariableTypes() {
		var body = Factory.GetMethodBody(_method.RelativeVirtualAddress);

		if (body.LocalSignature.IsNil) {
			return [];
		}

		var localSignature = Factory.GetStandaloneSignature(body.LocalSignature);
		var blob = Factory.GetLocalSignatureBlob(localSignature.Signature);
		return blob.ArgumentTypes;
	}
	
	public override string ToString() {
		var arguments = GetArgumentTypes();
		var argsString = string.Join(", ", arguments);
		return $"{ReturnType} {FullName}({argsString})";
	}

	public CILMethodDefinition GetMethodDefinition() => this;
}
