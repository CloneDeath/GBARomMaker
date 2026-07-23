using System.Reflection.Metadata;
using System.Reflection.PortableExecutable;

namespace GBARomMaker.CIL;

public class CILFieldDefinition {
	private readonly PEReader peReader;
	private readonly MetadataReader metadata;
	private readonly FieldDefinition _field;
	private readonly CILTypeDefinition _parent;
	
	public CILFieldDefinition(PEReader peReader, MetadataReader metadata, FieldDefinition field) {
		this.peReader = peReader;
		this.metadata = metadata;
		this._field = field;

		var factory = new CILFactory(peReader, metadata);
		_parent = factory.GetTypeDefinition(_field.GetDeclaringType());
	}


	public string Name => metadata.GetString(_field.Name);

	public string FullName => $"{_parent.FullName}.{Name}";
}
