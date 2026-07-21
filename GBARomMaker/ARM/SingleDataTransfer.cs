using GBARomMaker.ARM.Common;

namespace GBARomMaker.ARM;

// https://problemkaputt.de/gbatek-arm-opcodes-memory-single-data-transfer-ldr-str-pld.htm
public class SingleDataTransfer : IInstruction {
	public SingleDataTransfer() {
		Condition = Condition.Always;
		Offset = new Memory.Immediate(0);
		PrePost = PrePost.Pre;
		UpDown = UpDown.Up;
		Word = true;
		WriteBack = false;
		LoadStore = LoadStore.Load;
		BaseRegister = 0;
		SourceDestinationRegister = 0;
	}

	public Condition Condition { get; set; }
	public Memory.IOffset Offset { get; set; }
	public PrePost PrePost { get; set; }
	public UpDown UpDown { get; set; }
	public bool Word { get; set; }
	public bool WriteBack { get; set; }
	public LoadStore LoadStore { get; set; }
	public byte BaseRegister { get; set; }
	public byte SourceDestinationRegister { get; set; }

	public byte[] ToBytes() {
		var data = new byte[4] { 0, 0, 0, 0 };
		data[3] |= (byte)(((byte)Condition << 4) & 0b11110000);
		data[3] |= (byte)(0b01 << 2); // Instruction
		data[3] |= (byte)((Offset.IsImmediate ? 0 : 1) << 1);
		data[3] |= (byte)(PrePost);
		data[2] |= (byte)((byte)UpDown << 7);
		data[2] |= (byte)((Word ? 0 : 1) << 6);
		data[2] |= (byte)((WriteBack ? 1 : 0) << 5);
		data[2] |= (byte)((byte)LoadStore << 4);
		data[2] |= (byte)(BaseRegister & 0b1111);
		data[1] |= (byte)((SourceDestinationRegister & 0b1111) << 4);

		var offsetData = Offset.ToBytes();
		data[1] |= (byte)(offsetData[1] & 0b1111);
		data[0] = offsetData[0];

		return data;
	}
}
