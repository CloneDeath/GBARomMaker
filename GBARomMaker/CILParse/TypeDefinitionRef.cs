using System;
using System.Linq;
using System.Reflection.Metadata;
using System.Reflection.PortableExecutable;

namespace GBARomMaker.CILParse;

public class TypeDefinitionRef {
	private readonly PEReader peReader;
	private readonly MetadataReader metadata;
	private readonly TypeDefinition self;
	
	public TypeDefinitionRef(PEReader peReader, MetadataReader metadata, TypeDefinition self) {
		this.peReader = peReader;
		this.metadata = metadata;
		this.self = self;
	}

	public string Namespace => metadata.GetString(self.Namespace);
	public string Name => metadata.GetString(self.Name);

	public int FieldCount {
		get {
			var fields = self.GetFields().Select(f => metadata.GetFieldDefinition(f));
			return fields.Count();
		}
	}
}
