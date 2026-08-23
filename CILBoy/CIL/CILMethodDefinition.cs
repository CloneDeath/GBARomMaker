using System.Reflection;
using System.Reflection.Metadata;
using CILBoy.CIL.Blobs;

namespace CILBoy.CIL;

public class CILMethodDefinition : ICILMethod {
    private readonly CILAssemblyFactory _factory;
    private readonly MethodDefinition _method;
	private readonly MethodSignatureBlob _signature;

	public CILMethodDefinition(CILAssemblyFactory factory, MethodDefinition method) {
        this._factory = factory;
        this._method = method;
		this._signature = factory.GetMethodSignatureBlob(method.Signature);
	}

	public ICILType Parent => _factory.GetTypeDefinition(_method.GetDeclaringType());
	public string Name => _factory.GetString(_method.Name);
	public string FullName => $"{Parent.Namespace}.{Parent.Name}.{Name}";
	public byte[] BodyBytes => _factory.GetMethodBody(_method.RelativeVirtualAddress).GetILBytes() ?? [];
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
			var module = _factory.GetModuleReference(import.Module);
			var moduleName = _factory.GetString(module.Name);
			return moduleName == "gba";
		}
	}

	public string NativeInvokeTarget {
		get {
			var import = _method.GetImport();
			return _factory.GetString(import.Name);
		}
	}

	public ISignatureType[] GetLocalVariableTypes() {
		var body = _factory.GetMethodBody(_method.RelativeVirtualAddress);

		if (body.LocalSignature.IsNil) {
			return [];
		}

		var localSignature = _factory.GetStandaloneSignature(body.LocalSignature);
		var blob = _factory.GetLocalSignatureBlob(localSignature.Signature);
		return blob.ArgumentTypes;
	}
	
	public override string ToString() {
		var arguments = GetArgumentTypes();
		var argsString = string.Join(", ", arguments);
		return $"{ReturnType} {FullName}({argsString})";
	}
}
