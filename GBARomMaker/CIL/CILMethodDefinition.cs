using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
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

	public SignatureTypeCode ReturnValue {
		get {
			var signature = _metadata.GetBlobReader(_method.Signature);
			var header = signature.ReadSignatureHeader();
			if (header.IsGeneric) signature.ReadCompressedInteger(); // generic parameter count
			signature.ReadCompressedInteger(); // normal parameter count
			return signature.ReadSignatureTypeCode();
		}
	}

    public bool HasReturnValue => ReturnValue != SignatureTypeCode.Void;

	public SignatureTypeCode[] GetLocalVariableTypes() {
		var body = _peReader.GetMethodBody(_method.RelativeVirtualAddress);

		if (body.LocalSignature.IsNil) {
			return [];
		}

		var localSignature = _metadata.GetStandaloneSignature(body.LocalSignature);
		var signatureReader = _metadata.GetBlobReader(localSignature.Signature);

		signatureReader.ReadSignatureHeader();
		var localVariableCount = signatureReader.ReadCompressedInteger();

		var types = new List<SignatureTypeCode>();
		for (int i = 0; i < localVariableCount; i++) {
			var type = signatureReader.ReadSignatureTypeCode();
			types.Add(type);
		}
		return types.ToArray();
	}
}
