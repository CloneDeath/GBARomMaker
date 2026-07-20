using System.Reflection.Metadata;
using System.Reflection.PortableExecutable;

public class MethodDefinitionRef {
	private readonly PEReader peReader;
	private readonly MetadataReader metadata;
	private readonly MethodDefinition method;
	
	private TypeDefinition parent => metadata.GetTypeDefinition(method.GetDeclaringType());

	public MethodDefinitionRef(PEReader peReader, MetadataReader metadata, MethodDefinition method) {
		this.peReader = peReader;
		this.metadata = metadata;
		this.method = method;
	}

	public string Namespace => metadata.GetString(parent.Namespace);
	public string Class => metadata.GetString(parent.Name);
	public string Name => metadata.GetString(method.Name);
	public string FullName => $"{Namespace}.{Class}.{Name}";

	public byte[] BodyBytes => peReader.GetMethodBody(method.RelativeVirtualAddress)?.GetILBytes() ?? [];
}
