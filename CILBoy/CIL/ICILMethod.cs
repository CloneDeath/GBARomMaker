namespace CILBoy.CIL;

public interface ICILMethod {
	public string Name { get; }
	public ICILType Parent { get; }
	public string FullName { get; }
    public int ParameterCount { get; }
	public bool IsInstance { get; }
	public ISignatureType ReturnType { get; }
	public bool HasReturnValue { get; }
	public ISignatureType[] GetArgumentTypes();

	public CILAssemblyFactory Factory { get; }

	public CILMethodDefinition GetMethodDefinition();
}
