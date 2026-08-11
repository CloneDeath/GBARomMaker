using System;
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

	public ISignatureType[] GetLocalVariableTypes() {
		var body = _peReader.GetMethodBody(_method.RelativeVirtualAddress);

		if (body.LocalSignature.IsNil) {
			return [];
		}

		var localSignature = _metadata.GetStandaloneSignature(body.LocalSignature);
		var reader = _metadata.GetBlobReader(localSignature.Signature);

		var header = reader.ReadSignatureHeader();
		if (header.IsGeneric) reader.ReadCompressedInteger(); // generic param count
		var localVariableCount = reader.ReadCompressedInteger();

		var types = new List<ISignatureType>();
		for (int i = 0; i < localVariableCount; i++) {
			types.Add(SignatureType.Read(ref reader));
		}
		if (reader.RemainingBytes != 0) throw new Exception($"Failed to read all {localVariableCount} local variables. {reader.RemainingBytes} bytes remain.\n\tParsed: [{string.Join(", ", types)}]");
		return types.ToArray();
	}
	
	public override string ToString() {
		var arguments = GetArgumentTypes();
		var argsString = string.Join(", ", arguments);
		return $"{ReturnType} {FullName}({argsString})";
	}
}
