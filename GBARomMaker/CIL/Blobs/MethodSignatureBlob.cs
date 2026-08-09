using System;
using System.Collections.Generic;
using System.Reflection.Metadata;

namespace GBARomMaker.CIL.Blobs;

public class MethodSignatureBlob {
	public MethodSignatureBlob(MetadataReader metadata, BlobHandle handle) {
		var reader = metadata.GetBlobReader(handle);
		var header = reader.ReadSignatureHeader();
		IsInstance = header.IsInstance;

		GenericParameterCount = header.IsGeneric
			? reader.ReadCompressedInteger()
			: 0;
		ParameterCount = reader.ReadCompressedInteger();
		ReturnType = SignatureType.Read(ref reader);

		var types = new List<ISignatureType>();
		for (var i = 0; i < ParameterCount; i++) {
			types.Add(SignatureType.Read(ref reader));
		}
		if (reader.RemainingBytes != 0) throw new Exception($"Failed to read all bytes from signature header. {reader.RemainingBytes} bytes remain.\n\tParsed: [{string.Join(", ", types)}]");
		ArgumentTypes = types.ToArray();
	}

	public int GenericParameterCount { get; }
	public int ParameterCount { get; }
	public ISignatureType ReturnType { get; }
	public ISignatureType[] ArgumentTypes { get; }
	public bool IsInstance { get; }
}
