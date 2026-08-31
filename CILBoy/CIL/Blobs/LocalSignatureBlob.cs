using System;
using System.Collections.Generic;
using System.Reflection.Metadata;

namespace CILBoy.CIL.Blobs;

public class LocalSignatureBlob {
	public LocalSignatureBlob(CILAssemblyFactory factory, BlobReader reader) {
		var header = reader.ReadSignatureHeader();

		GenericParameterCount = header.IsGeneric
			? reader.ReadCompressedInteger()
			: 0;
		ParameterCount = reader.ReadCompressedInteger();

		var types = new List<ISignatureType>();
		for (var i = 0; i < ParameterCount; i++) {
			types.Add(SignatureType.Read(factory, ref reader));
		}
		if (reader.RemainingBytes != 0) throw new Exception($"Failed to read all bytes from signature header. {reader.RemainingBytes} bytes remain."
			+ $"\n\tArgumentTypes: [{string.Join(", ", types)}]");
		ArgumentTypes = types.ToArray();
	}

	public int GenericParameterCount { get; }
	public int ParameterCount { get; }
	public ISignatureType[] ArgumentTypes { get; }
}
