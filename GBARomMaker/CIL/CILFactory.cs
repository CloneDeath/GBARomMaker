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

	public ICILType GetTypeDefinition(int metadataToken) {
		var handle = MetadataTokens.EntityHandle(metadataToken);
		switch (handle.Kind) {
			case HandleKind.TypeDefinition: {
				return GetTypeDefinition((TypeDefinitionHandle)handle);
			}
			case HandleKind.TypeReference: {
				return GetTypeReference((TypeReferenceHandle)handle);
			}
			default: throw new NotImplementedException($"Tried to extract Type from {handle.Kind}");
		}
	}

    public ICILMethod GetMethodDefinition(int metadataToken) {
		var handle = MetadataTokens.EntityHandle(metadataToken);
		return GetMethodDefinition(handle);
    }

	public ICILMethod GetMethodDefinition(EntityHandle handle) {
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
			case HandleKind.MethodSpecification: {
				var member = _metadata.GetMethodSpecification((MethodSpecificationHandle)handle);
				return GetMethodDefinition(member.Method);
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

	public string GetString(int metadataToken) {
		var handle = MetadataTokens.UserStringHandle(metadataToken);
		return _metadata.GetUserString(handle);
	}
}
