using System.Reflection.Metadata;
using System.Reflection.PortableExecutable;

namespace GBARomMaker.CIL;

public class CILFieldDefinition {
	private readonly PEReader _peReader;
	private readonly MetadataReader _metadata;
	private readonly FieldDefinition _field;
	
	public CILFieldDefinition(PEReader peReader, MetadataReader metadata, FieldDefinition field) {
		this._peReader = peReader;
		this._metadata = metadata;
		this._field = field;

		var factory = new CILFactory(peReader, metadata);
		Parent = factory.GetTypeDefinition(_field.GetDeclaringType());
	}

	public CILTypeDefinition Parent { get; }

	public string Name => _metadata.GetString(_field.Name);
	public string FullName => $"{Parent.FullName}.{Name}";
	public SignatureTypeCode Type {
		get {
			var signature = _metadata.GetBlobReader(_field.Signature);
			signature.ReadSignatureHeader();

			return signature.ReadSignatureTypeCode();
		}
	}
}
