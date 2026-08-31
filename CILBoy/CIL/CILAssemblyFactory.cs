using System;
using System.IO;
using System.Linq;
using System.Reflection.Metadata;
using System.Reflection.Metadata.Ecma335;
using System.Reflection.PortableExecutable;
using CILBoy.CIL.Blobs;

namespace CILBoy.CIL;

public class CILAssemblyFactory : IDisposable {
	private readonly CILFactory _factory;
	private readonly FileStream _stream;
	private readonly PEReader _peReader;
	private readonly MetadataReader _metadata;

	public string AssemblyName { get; private set; }

	public CILAssemblyFactory(CILFactory factory, string filePath) {
		_factory = factory;
		AssemblyName = Path.GetFileNameWithoutExtension(filePath);
		_stream = File.OpenRead(filePath);
		_peReader = new PEReader(_stream);
		_metadata = _peReader.GetMetadataReader();
	}

	public void Dispose() {
		_peReader.Dispose();
		_stream.Dispose();
	}

	public PEHeaders PEHeaders => _peReader.PEHeaders;

	public CILTypeDefinition GetTypeDefinition(string fullName) {
		return _metadata.TypeDefinitions
			.Select(t => GetTypeDefinition(t))
			.First(d => d.FullName == fullName);
	}

	public CILTypeDefinition GetTypeDefinition(TypeDefinitionHandle handle) {
		var typeDefinition = _metadata.GetTypeDefinition(handle);
		return new CILTypeDefinition(this, typeDefinition);
	}
	
	public ICILType GetTypeReference(TypeReferenceHandle handle) {
		var typeReference = new CILTypeReference(_peReader, _metadata, _metadata.GetTypeReference(handle));
		if (typeReference.Assembly.Name.StartsWith("System.")) return typeReference;

		var definingFactory = _factory.GetAssemblyFactoryFor(typeReference.Assembly.Name);
		return definingFactory.GetTypeDefinition(typeReference.FullName);
	}

	public ICILType GetTypeDefinition(EntityHandle handle) {
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

	public ICILType GetTypeDefinition(int metadataToken) {
		var handle = MetadataTokens.EntityHandle(metadataToken);
		return GetTypeDefinition(handle);
	}

    public ICILMethod GetMethodDefinition(int metadataToken) {
		var handle = MetadataTokens.EntityHandle(metadataToken);
		return GetMethodDefinition(handle);
    }

	public CILMethodDefinition GetMethodDefinition(MethodDefinitionHandle handle) {
		var method = _metadata.GetMethodDefinition(handle);
		return new CILMethodDefinition(this, method);
	}

	public ICILMethod GetMethodDefinition(MemberReferenceHandle handle) {
		var member = _metadata.GetMemberReference((MemberReferenceHandle)handle);
		var memberRef = new CILMemberReference(this, member);
		if (memberRef.Kind != MemberReferenceKind.Method) throw new Exception($"Could not extract a Method from a member ref to a {memberRef.Kind}");
		return memberRef;
	}

	public ICILMethod GetMethodDefinition(EntityHandle handle) {
		switch (handle.Kind) {
			case HandleKind.MethodDefinition: {
				return GetMethodDefinition((MethodDefinitionHandle)handle);
			}
			case HandleKind.MemberReference: {
				return GetMethodDefinition((MemberReferenceHandle)handle);
			}
			case HandleKind.MethodSpecification: {
				var member = _metadata.GetMethodSpecification((MethodSpecificationHandle)handle);
				return GetMethodDefinition(member.Method);
			}
			default: throw new NotImplementedException($"Tried to extract Method from {handle.Kind}");
		}
	}

	public CILFieldDefinition GetFieldDefinition(FieldDefinitionHandle handle) {
		var field = _metadata.GetFieldDefinition(handle);
		return new CILFieldDefinition(this, field);
	}

	public CILFieldDefinition GetFieldDefinition(MemberReferenceHandle handle) {
		var member = _metadata.GetMemberReference(handle);
		var parent = GetTypeDefinition(member.Parent);
		var memberName = _metadata.GetString(member.Name);
		return parent.InstanceFields.FirstOrDefault(f => f.Name == memberName)
			?? parent.StaticFields.First(f => f.Name == memberName);
	}

	public CILFieldDefinition GetFieldDefinition(int metadataToken) {
		var handle = MetadataTokens.EntityHandle(metadataToken);
		switch (handle.Kind) {
			case HandleKind.FieldDefinition: {
				return GetFieldDefinition((FieldDefinitionHandle)handle);
			}
			case HandleKind.MemberReference: {
				return GetFieldDefinition((MemberReferenceHandle)handle);
			}
			default: throw new NotImplementedException($"Could not extract field from {handle.Kind}");
		}
	}

	public string GetString(StringHandle handle) => _metadata.GetString(handle);
	public string GetUserString(int metadataToken) {
		var handle = MetadataTokens.UserStringHandle(metadataToken);
		return _metadata.GetUserString(handle);
	}

	public BlobReader GetBlobReader(BlobHandle handle) => _metadata.GetBlobReader(handle);

	public MethodSignatureBlob GetMethodSignatureBlob(BlobHandle signature) => new MethodSignatureBlob(this, _metadata.GetBlobReader(signature));
	public LocalSignatureBlob GetLocalSignatureBlob(BlobHandle signature) => new LocalSignatureBlob(this, _metadata.GetBlobReader(signature));

	public MethodBodyBlock GetMethodBody(int relativeVirtualAddress) => _peReader.GetMethodBody(relativeVirtualAddress);

	public ModuleReference GetModuleReference(ModuleReferenceHandle handle) => _metadata.GetModuleReference(handle);
	public StandaloneSignature GetStandaloneSignature(StandaloneSignatureHandle handle) => _metadata.GetStandaloneSignature(handle);

	public CILAssemblyFactory GetAssemblyFactoryFor(string assembly) => _factory.GetAssemblyFactoryFor(assembly);

	public override string ToString() => $"{nameof(CILAssemblyFactory)}{{{AssemblyName}}}";
}
