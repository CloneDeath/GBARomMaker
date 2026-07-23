using System;
using System.Reflection.Metadata;
using System.Reflection.Metadata.Ecma335;
using System.Reflection.PortableExecutable;

namespace GBARomMaker.CIL;

public class CILFactory {
	private readonly PEReader _peReader;
	private readonly MetadataReader _metadata;

	public CILFactory(PEReader peReader, MetadataReader metadata) {
		_peReader = peReader;
		_metadata = metadata;
	}

	public CILTypeDefinition GetTypeDefinition(TypeDefinitionHandle handle) {
		var typeDefinition = _metadata.GetTypeDefinition(handle);
		return new CILTypeDefinition(_peReader, _metadata, typeDefinition);
	}

	public CILTypeReference GetTypeReference(TypeReferenceHandle handle) {
		var typeReference = _metadata.GetTypeReference(handle);
		return new CILTypeReference(_peReader, _metadata, typeReference);
	}

    public ICILMethod GetMethodDefinition(int metadataToken) {
		var handle = MetadataTokens.EntityHandle(metadataToken);
		switch (handle.Kind) {
			case HandleKind.MethodDefinition: {
				var method = _metadata.GetMethodDefinition((MethodDefinitionHandle)handle);
				return new CILMethodDefinition(_peReader, _metadata, method);
			}
			case HandleKind.MemberReference: {
				var member = _metadata.GetMemberReference((MemberReferenceHandle)handle);
				var memberRef = new CILMemberReference(_peReader, _metadata, member);
				if (memberRef.Kind != MemberReferenceKind.Method) throw new Exception($"Could not extract a Method from a member ref to a {memberRef.Kind}");
				return memberRef;
			}
			default: throw new NotImplementedException($"Tried to extract Method from {handle.Kind}");
		}
    }

	public CILFieldDefinition GetFieldDefinition(int metadataToken) {
		var handle = MetadataTokens.EntityHandle(metadataToken);
		switch (handle.Kind) {
			case HandleKind.FieldDefinition: {
				var field = _metadata.GetFieldDefinition((FieldDefinitionHandle)handle);
				return new CILFieldDefinition(_peReader, _metadata, field);
			}
			default: throw new NotImplementedException($"Could not extract field from {handle.Kind}");
		}
	}
}
