using System;

namespace GBARomMaker.Rom.Operations;

public enum AddOffset {
	PreTransfer = 1,
	PostTransfer = 0
}

public enum BaseOperation {
	Subtract = 0,
	Down = 0,
	Add = 1,
	Up = 1
}

public enum LoadStore {
	Load = 1,
	Store = 0
}

// https://problemkaputt.de/gbatek-arm-opcodes-memory-halfword-doubleword-and-signed-data-transfer.htm
public class StoreHalf : Operation {
	public StoreHalf() {
		Condition = Condition.Always;
		AddOffset = AddOffset.PreTransfer;
		BaseOperation = BaseOperation.Add;
		ImmediateOffset = true;
		WriteBack = false;
		LoadStore = LoadStore.Store;
		BaseRegister = 0;
		DestinationRegister = 0;
	}

	public StoreHalf(byte[] data) {
		Condition = (Condition)(data[3] >> 4);
		if (((data[3] >> 1) & 0b111) != 0b000) {
			throw new Exception("Invalid instruction for a STRH");
		}
		AddOffset = (data[3] & 0b1) == 1 ? AddOffset.PreTransfer : AddOffset.PostTransfer;
		BaseOperation = (BaseOperation)(data[2] >> 7);
		ImmediateOffset = ((data[2] >> 6) & 0b1) == 1;
		WriteBack = ((data[2] >> 5) & 0b1) == 1;
		if (AddOffset == AddOffset.PostTransfer && WriteBack) {
			throw new Exception("Writeback was set, but add offset was set to post transfer...");
		}
		LoadStore = (LoadStore)((data[2] >> 4) & 0b1);
		BaseRegister = (byte)(data[2] & 0b1111);
		DestinationRegister = (byte)(data[1] >> 4);

		// TODO bits 11-8
	}

	public Condition Condition { get; set; }
	public Instruction Instruction { get; } = Instruction.StoreHalf;
	public AddOffset AddOffset { get; set; }
	public BaseOperation BaseOperation { get; set; }
	public bool ImmediateOffset { get; set; }
	public bool WriteBack { get; set; }
	public LoadStore LoadStore { get; set; }
	public byte BaseRegister { get; set; }
	public byte DestinationRegister { get; set; }

	public override byte[] ToBytes() {
		var data = new byte[4] { 0, 0, 0, 0 };
		data[3] |= (byte)(((byte)Condition << 4) & 0b11110000);
		// instruction is 0b000, which is already set for data[3]...
		data[3] |= (byte)(AddOffset);
		data[2] |= (byte)((byte)AddOffset << 7);
		data[2] |= (byte)((ImmediateOffset ? 1 : 0) << 6);
		data[2] |= (byte)((WriteBack ? 1 : 0) << 5);
		data[2] |= (byte)(((byte)LoadStore) << 4);
		data[2] |= BaseRegister;
		data[1] |= (byte)(DestinationRegister << 4);
	}

}
