using System.Linq;
using System.Reflection.Metadata;

namespace CILBoy.CIL;

public class CILTypeDefinition : ICILType {
	private readonly CILAssemblyFactory _factory;
	private readonly TypeDefinition _self;
	
	public CILTypeDefinition(CILAssemblyFactory factory, TypeDefinition self) {
		this._factory = factory;
		this._self = self;
	}

	public string Namespace => _factory.GetString(_self.Namespace);
	public string Name => _factory.GetString(_self.Name);
	public string FullName => $"{Namespace}.{Name}";

    public CILMethodDefinition? StaticConstructor => _self.GetMethods()
		.Select(m => _factory.GetMethodDefinition(m))
		.FirstOrDefault(m => m.IsStaticConstructor);

    public CILMethodDefinition GetMethodDefinition(string name) {
		var methods = _self.GetMethods().Select(m => _factory.GetMethodDefinition(m));
		return methods.First(m => m.Name == name);
	}

	public CILFieldDefinition[] InstanceFields => _self.GetFields().Select(f => _factory.GetFieldDefinition(f)).Where(f => !f.IsStatic).ToArray();
	public CILFieldDefinition[] StaticFields => _self.GetFields().Select(f => _factory.GetFieldDefinition(f)).Where(f => f.IsStatic).ToArray();

	public override string ToString() => $"{nameof(CILTypeDefinition)}{{{FullName}}}";
}
