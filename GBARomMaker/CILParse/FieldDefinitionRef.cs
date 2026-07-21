using System.Reflection.Metadata;
using System.Reflection.PortableExecutable;

namespace GBARomMaker.CILParse;

public class FieldDefinitionRef {
	private readonly PEReader peReader;
	private readonly MetadataReader metadata;
	private readonly FieldDefinition _field;
	
	public FieldDefinitionRef(PEReader peReader, MetadataReader metadata, FieldDefinition field) {
		this.peReader = peReader;
		this.metadata = metadata;
		this._field = field;
	}

	public string Name => metadata.GetString(_field.Name);
}
