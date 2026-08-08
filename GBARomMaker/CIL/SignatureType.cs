using System.Reflection.Metadata;

namespace GBARomMaker.CIL;

public interface ISignatureType {
	public SignatureTypeCode Code { get; }
}

public class SignatureType(SignatureTypeCode code) : ISignatureType {
	public SignatureTypeCode Code => code;
}

public class ArraySignatureType(ISignatureType innerType) : ISignatureType {
	public SignatureTypeCode Code => SignatureTypeCode.SZArray;
	public ISignatureType InnerType => innerType;
}
