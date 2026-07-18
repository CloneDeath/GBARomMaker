namespace GBARomMaker.ARM;

public enum PrePost {
	Pre = 1,
	Post = 0
}

public enum UpDown {
	Up = 1,
	Down = 0
}

// https://problemkaputt.de/gbatek-arm-opcodes-memory-block-data-transfer-ldm-stm.htm
public class BlockDataTransfer : IInstruction {
	public BlockDataTransfer() {
		Condition = Condition.Always;
		PrePost = PrePost.Post;
		UpDown = UpDown.Up;
		ForceUser = false;
		WriteBack = false;
		LoadStore = LoadStore.Load;
		BaseRegister = 13;
		RegisterList = 0;
	}

	public Condition Condition { get; set; }
	public PrePost PrePost { get; set; }
	public UpDown UpDown { get; set; }
	public bool ForceUser { get; set; }
	public bool WriteBack { get; set; }
	public LoadStore LoadStore { get; set; }
	public byte BaseRegister { get; set; }
	public ushort RegisterList { get; set; }

	public byte[] ToBytes() {
		var data = new byte[4] { 0, 0, 0, 0 };
		data[3] |= (byte)(((byte)Condition << 4) & 0b11110000);
		data[3] |= 0b100 << 1; // instruction
		data[3] |= (byte)((byte)PrePost & 0b1);
		data[2] |= (byte)(((byte)UpDown & 0b1) << 7);
		data[2] |= (byte)((ForceUser ? 1 : 0) << 6);
		data[2] |= (byte)((WriteBack ? 1 : 0) << 5);
		data[2] |= (byte)(((byte)LoadStore) << 4);
		data[2] |= (byte)((BaseRegister) & 0b1111);
		data[1] = (byte)(RegisterList >> 8);
		data[0] = (byte)(RegisterList & 0xFF);
		return data;
	}

}
