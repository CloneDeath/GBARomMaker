using System.Collections.Generic;
using System.Reflection.Metadata;

namespace GBARomMaker.CIL.Blobs;

public class MethodSignatureBlob {
	public MethodSignatureBlob(MetadataReader metadata, BlobHandle handle) {
		var signature = metadata.GetBlobReader(handle);
		var header = signature.ReadSignatureHeader();
		IsInstance = header.IsInstance;

		GenericParameterCount = header.IsGeneric
			? signature.ReadCompressedInteger()
			: 0;
		ParameterCount = signature.ReadCompressedInteger();
		ReturnType = signature.ReadSignatureTypeCode();

		var types = new List<SignatureTypeCode>();
		for (var i = 0; i < ParameterCount; i++) {
			types.Add(signature.ReadSignatureTypeCode());
		}
		ArgumentTypes = types.ToArray();
	}

	public int GenericParameterCount { get; }
	public int ParameterCount { get; }
	public SignatureTypeCode ReturnType { get; }
	public SignatureTypeCode[] ArgumentTypes { get; }
	public bool IsInstance { get; }
}
