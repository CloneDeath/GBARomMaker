namespace GBARomMaker;

public class RomReader {
	private byte[] _data;
	public RomReader(byte[] data) {
		this._data = data;
	}

	public Header Header => new Header(_data[..192]);
}
