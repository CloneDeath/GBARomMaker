using System.Reflection.Metadata;
using System.Reflection.PortableExecutable;

namespace GBARomMaker.CIL;

public class CILFieldDefinition {
	private readonly PEReader peReader;
	private readonly MetadataReader metadata;
	private readonly FieldDefinition _field;
	
	public CILFieldDefinition(PEReader peReader, MetadataReader metadata, FieldDefinition field) {
		this.peReader = peReader;
		this.metadata = metadata;
		this._field = field;

		var factory = new CILFactory(peReader, metadata);
		Parent = factory.GetTypeDefinition(_field.GetDeclaringType());
	}

	public CILTypeDefinition Parent { get; }

	public string Name => metadata.GetString(_field.Name);
	public string FullName => $"{Parent.FullName}.{Name}";
}
