namespace CILBoy.CIL;

public interface ICILType {
	public string Name { get; }
	public string Namespace { get; }
	public string FullName { get; }
	public CILFieldDefinition[] InstanceFields { get; }
	public CILFieldDefinition[] StaticFields { get; }
}
