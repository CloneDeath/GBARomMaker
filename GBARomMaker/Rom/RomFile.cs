using System.Linq;

namespace GBARomMaker.Rom;

public class RomFile {
	public readonly Header Header;

	public RomFile() {
		Header = new Header();
		Content = new byte[0];
	}

	public RomFile(byte[] data) {
		Header = new Header(data[..192]);
		Content = data[192..];
	}

	public byte[] Content;
	
	public byte[] ToBytes(bool calculateChecksum = true) {
		return Header.ToBytes(calculateChecksum).Concat(Content).ToArray();
	}
}
