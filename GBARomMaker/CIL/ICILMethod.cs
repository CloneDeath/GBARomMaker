namespace GBARomMaker.CIL;

public interface ICILMethod {
	public string Name { get; }
	public string FullName { get; }
	public byte[] BodyBytes { get; }
    int ParameterCount { get; }
	bool IsInstance { get; }
}
