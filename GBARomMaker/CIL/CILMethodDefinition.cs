using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Metadata;
using System.Reflection.PortableExecutable;
using GBARomMaker.CIL.Blobs;

namespace GBARomMaker.CIL;

public class CILMethodDefinition : ICILMethod {
	private readonly PEReader _peReader;
	private readonly MetadataReader _metadata;
	private readonly MethodDefinition _method;
	private readonly MethodSignatureBlob _signature;

	public CILMethodDefinition(PEReader peReader, MetadataReader metadata, MethodDefinition method) {
		this._peReader = peReader;
		this._metadata = metadata;
		this._method = method;
		this._signature = new MethodSignatureBlob(metadata, method.Signature);
	}

	public CILTypeDefinition Parent => new(_peReader, _metadata, _metadata.GetTypeDefinition(_method.GetDeclaringType()));
	public string Name => _metadata.GetString(_method.Name);
	public string FullName => $"{Parent.Namespace}.{Parent.Name}.{Name}";
	public byte[] BodyBytes => _peReader.GetMethodBody(_method.RelativeVirtualAddress)?.GetILBytes() ?? [];
	public bool IsInstance => _signature.IsInstance;
	public int ParameterCount => _signature.ParameterCount;
	public SignatureTypeCode ReturnType => _signature.ReturnType;
	public bool HasReturnValue => ReturnType != SignatureTypeCode.Void;
	public SignatureTypeCode[] GetArgumentTypes() => _signature.ArgumentTypes;

	public bool IsConstructor => IsInstanceConstructor || IsStaticConstructor;
	public bool IsInstanceConstructor => Name == ".ctor";
	public bool IsStaticConstructor => Name == ".cctor";

	public bool IsNativeInvoke {
		get {
			var isInvoke = _method.Attributes.HasFlag(MethodAttributes.PinvokeImpl);
			if (!isInvoke) return false;

			var import = _method.GetImport();
			var module = _metadata.GetModuleReference(import.Module);
			var moduleName = _metadata.GetString(module.Name);
			return moduleName == "gba";
		}
	}

	public string NativeInvokeTarget {
		get {
			var import = _method.GetImport();
			return _metadata.GetString(import.Name);
		}
	}

	public SignatureTypeCode[] GetLocalVariableTypes() {
		var body = _peReader.GetMethodBody(_method.RelativeVirtualAddress);

		if (body.LocalSignature.IsNil) {
			return [];
		}

		var localSignature = _metadata.GetStandaloneSignature(body.LocalSignature);
		var signatureReader = _metadata.GetBlobReader(localSignature.Signature);

		var header = signatureReader.ReadSignatureHeader();
		if (header.IsGeneric) signatureReader.ReadCompressedInteger(); // generic param count
		var localVariableCount = signatureReader.ReadCompressedInteger();

		var types = new List<SignatureTypeCode>();
		for (int i = 0; i < localVariableCount; i++) {
			var type = signatureReader.ReadSignatureTypeCode();
			if (type == SignatureTypeCode.TypeHandle) {
				// https://learn.microsoft.com/en-us/dotnet/api/system.reflection.metadata.signaturetypecode?view=net-11.0-pp
				signatureReader.ReadSignatureTypeCode(); // todo figure out the actual of the type we just referenced
				types.Add(type);
			} else if (type == SignatureTypeCode.Pointer) {
				signatureReader.ReadSignatureTypeCode();
				types.Add(type); // todo Do we need the referenced type too?
			} else {
				types.Add(type);
			}
		}
		return types.ToArray();
	}
}
