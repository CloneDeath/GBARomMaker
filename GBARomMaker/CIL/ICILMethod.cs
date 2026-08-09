namespace GBARomMaker.CIL;

public interface ICILMethod {
	public string Name { get; }
	public string FullName { get; }
	public byte[] BodyBytes { get; }
    public int ParameterCount { get; }
	public bool IsInstance { get; }
	public bool IsNativeInvoke { get; }
	public string NativeInvokeTarget { get; }
	public ISignatureType ReturnType { get; }
	public bool HasReturnValue { get; }
	public ISignatureType[] GetArgumentTypes();
	public ISignatureType[] GetLocalVariableTypes();
}
