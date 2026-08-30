using System.Reflection.Metadata;

namespace CILBoy.CIL;

public class CILFieldDefinition {
	private readonly CILAssemblyFactory _factory;
	private readonly FieldDefinition _field;
	
	public CILFieldDefinition(CILAssemblyFactory factory, FieldDefinition field) {
		this._factory = factory;
		this._field = field;
		Parent = factory.GetTypeDefinition(_field.GetDeclaringType());
	}

	public CILTypeDefinition Parent { get; }

	public string Name => _factory.GetString(_field.Name);
	public string FullName => $"{Parent.FullName}.{Name}";
	public SignatureTypeCode Type {
		get {
			System.Console.WriteLine("field dump");
			System.Console.WriteLine(_factory);
			System.Console.WriteLine(Parent);
			System.Console.WriteLine(Name);
			var signature = _factory.GetBlobReader(_field.Signature);
			System.Console.WriteLine("b");
			signature.ReadSignatureHeader();

			return signature.ReadSignatureTypeCode();
		}
	}

	public bool IsStatic => _field.Attributes.HasFlag(System.Reflection.FieldAttributes.Static);
}
