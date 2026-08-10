using System.Reflection.Metadata;

namespace GBARomMaker.CIL;

public interface ISignatureType {
	public SignatureTypeCode Code { get; }
}

public class SignatureType(SignatureTypeCode code) : ISignatureType {
	public SignatureTypeCode Code => code;

	public override string ToString() => $"{Code}";

	public static ISignatureType Read(ref BlobReader reader) {
		var type = reader.ReadSignatureTypeCode();
		if (type == SignatureTypeCode.SZArray) {
			var innerType = reader.ReadSignatureTypeCode();
			// TODO properly reference object types
			if (innerType == SignatureTypeCode.TypeHandle) {
				var innerInnerType = reader.ReadSignatureTypeCode();
			}
			return new ArraySignatureType(new SignatureType(innerType));
		} else if (type == SignatureTypeCode.TypeHandle) {
			// https://learn.microsoft.com/en-us/dotnet/api/system.reflection.metadata.signaturetypecode?view=net-11.0-pp
			// todo figure out the actual of the type we just referenced
			var skipped = reader.ReadSignatureTypeCode();
			return new SignatureType(type);
		} else if (type == SignatureTypeCode.Pointer) {
			// todo Do we need the referenced type too?
			var skipped = reader.ReadSignatureTypeCode();
			return new SignatureType(type);
		} else if (type == SignatureTypeCode.GenericMethodParameter) {
			var token = reader.ReadCompressedInteger();
			return new GenericSignatureType(token);
		}
		return new SignatureType(type);
	}
}

public class ArraySignatureType(ISignatureType innerType) : ISignatureType {
	public SignatureTypeCode Code => SignatureTypeCode.SZArray;
	public ISignatureType InnerType => innerType;


	public override string ToString() => $"{Code}<{InnerType}>";
}

public class GenericSignatureType(int metadataToken) : ISignatureType {
	public SignatureTypeCode Code => SignatureTypeCode.GenericMethodParameter;

	public int MetadataToken => metadataToken;
	public override string ToString() => $"{Code}<{MetadataToken}>";
}
