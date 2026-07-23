namespace GBARomMaker.CIL;

public interface ICILMethod {
	public string Name { get; }
	public string FullName { get; }
	public byte[] BodyBytes { get; }
    bool IsInstanceMethod { get; }
    int ArgumentCount { get; }
}
