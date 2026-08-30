namespace CILBoy.CIL;

public interface ICILType {
	public string Name { get; }
	public string Namespace { get; }
	public string FullName { get; }
	public CILMethodDefinition GetMethodDefinition(string name);
	public CILFieldDefinition[] InstanceFields { get; }
	public CILFieldDefinition[] StaticFields { get; }
}
