using System.Text;

namespace GBARomMaker;

public class Header(byte[] data) {
	public OpCode EntryPoint => new OpCode(data[0..4]);

	public byte[] NintendoLogo => data[0x4..(0x4 + 156)];
	public string GameTitle => Encoding.ASCII.GetString(data[0xA0..(0xA0 + 12)]);
	public string GameCode => Encoding.ASCII.GetString(data[0xAC..(0xAC + 4)]);
	public string MakerCode => Encoding.ASCII.GetString(data[0xB0..(0xB0 + 2)]);
}
