using System;
using System.Reflection.Metadata;
using System.Reflection.PortableExecutable;

namespace GBARomMaker.CIL;

public class MemberReferenceRef {
	private readonly PEReader peReader;
	private readonly MetadataReader metadata;
	private readonly MemberReference self;
	
	public MemberReferenceRef(PEReader peReader, MetadataReader metadata, MemberReference self) {
		this.peReader = peReader;
		this.metadata = metadata;
		this.self = self;
	}

	public string Name => metadata.GetString(self.Name);

	public MemberReferenceKind Kind => self.GetKind();

    public TypeDefinitionRef Parent {
        get {
			switch (self.Parent.Kind) {
				case HandleKind.TypeDefinition: {
					var typeDefinition = metadata.GetTypeDefinition((TypeDefinitionHandle)self.Parent);
					return new TypeDefinitionRef(peReader, metadata, typeDefinition);
				}
				case HandleKind.TypeReference: {
					var factory = new CILFactory(peReader, metadata);
					return factory.GetTypeDefinition((TypeReferenceHandle)self.Parent);
				}
				default: {
					throw new NotImplementedException($"Unexpected parent handle kind: {self.Parent.Kind}");
				}
			}
        }
    }

	public MethodDefinitionRef MethodDefinition {
		get {
			return Parent.GetMethodDefinition(Name);
		}
	}
}
