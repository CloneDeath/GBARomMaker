using System;
using System.Reflection.Metadata;
using System.Reflection.PortableExecutable;

namespace GBARomMaker.CIL;

public class CILFactory {
	private readonly PEReader _peReader;
	private readonly MetadataReader _metadata;

	public CILFactory(PEReader peReader, MetadataReader metadata) {
		_peReader = peReader;
		_metadata = metadata;
	}

	public TypeDefinitionRef GetTypeDefinition(TypeReferenceHandle handle) {
		var typeReference = _metadata.GetTypeReference(handle);

		Console.WriteLine(_metadata.GetString(typeReference.Name));
		Console.WriteLine(_metadata.GetString(typeReference.Namespace));
		
		switch (typeReference.ResolutionScope.Kind) {
			case HandleKind.AssemblyReference: {
				var assembly = _metadata.GetAssemblyReference((AssemblyReferenceHandle)typeReference.ResolutionScope);
				Console.WriteLine(_metadata.GetString(assembly.Name));

				throw new Exception("abc");

			}
			default: throw new NotImplementedException($"Can not get Type from ResolutionScope of kind {typeReference.ResolutionScope.Kind}");
		}
	}
}
