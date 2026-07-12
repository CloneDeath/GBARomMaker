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

public enum HOpCode {
	Reserved,
	SWP,
	STRH,
	LDRD,
	STRD,
	LDRH,
	LDRSB,
	LDRSH
}

// https://problemkaputt.de/gbatek-arm-opcodes-memory-halfword-doubleword-and-signed-data-transfer.htm
public class MemoryHalf : Operation {
	public MemoryHalf() {
		Condition = Condition.Always;
		AddOffset = AddOffset.PreTransfer;
		BaseOperation = BaseOperation.Add;
		ImmediateOffsetFlag = true;
		WriteBack = false;
		BaseRegister = 0;
		DestinationRegister = 0;
		ImmediateOffset = 0;
		Reserved1 = true;
		OpCode = HOpCode.STRH;
		Reserved2 = true;
		OffsetRegister = 0;
	}

	public MemoryHalf(byte[] data) {
		Condition = (Condition)(data[3] >> 4);
		if (((data[3] >> 1) & 0b111) != 0b000) {
			throw new Exception("Invalid instruction for a STRH");
		}
		AddOffset = (data[3] & 0b1) == 1 ? AddOffset.PreTransfer : AddOffset.PostTransfer;
		BaseOperation = (BaseOperation)(data[2] >> 7);
		ImmediateOffsetFlag = ((data[2] >> 6) & 0b1) == 1;
		WriteBack = ((data[2] >> 5) & 0b1) == 1;
		var loadStore = (LoadStore)((data[2] >> 4) & 0b1);
		BaseRegister = (byte)(data[2] & 0b1111);
		DestinationRegister = (byte)(data[1] >> 4);
		ImmediateOffset = ImmediateOffsetFlag ? (byte)(((data[1] & 0b1111) << 4) | (data[0] & 0b1111)) : (byte)0;
		Reserved1 = (data[0] >> 7) == 1;
		OpCode = ((data[0] >> 5) & 0b11) switch {
			0 => loadStore == LoadStore.Store ? HOpCode.SWP : HOpCode.Reserved,
			1 => loadStore == LoadStore.Store ? HOpCode.STRH : HOpCode.LDRH,
			2 => loadStore == LoadStore.Store ? HOpCode.LDRD : HOpCode.LDRSB,
			3 => loadStore == LoadStore.Store ? HOpCode.STRD : HOpCode.LDRSH,
			_ => throw new Exception("Invalid value for opcode")
		};
		Reserved2 = ((data[0] >> 4) & 0b1) == 1;
		OffsetRegister = ImmediateOffsetFlag ? (byte)0 : (byte)(data[0] & 0b1111);
	}

	public Condition Condition { get; set; }
	public Instruction Instruction { get; } = Instruction.StoreHalf;
	public AddOffset AddOffset { get; set; }
	public BaseOperation BaseOperation { get; set; }
	public bool ImmediateOffsetFlag { get; set; }
	public bool WriteBack { get; set; }
	public byte BaseRegister { get; set; }
	public byte DestinationRegister { get; set; }
	public byte ImmediateOffset { get; set; }
	public bool Reserved1 { get; set; }
	public HOpCode OpCode { get; set; }
	public bool Reserved2 { get; set; }
	public byte OffsetRegister { get; set; }

	public override byte[] ToBytes() {
		if (AddOffset == AddOffset.PostTransfer && WriteBack) {
			throw new Exception("Writeback was set, but add offset was set to post transfer...");
		}
		if (!ImmediateOffsetFlag && ImmediateOffset != 0) throw new Exception("ImmediateOffsetFlag not set, but a value was set for ImmidiateOffset");
		if (!Reserved1) throw new Exception("Reserved bit must be 1");
		if (!Reserved2) throw new Exception("Reserved bit must be 1");

		var data = new byte[4] { 0, 0, 0, 0 };
		data[3] |= (byte)(((byte)Condition << 4) & 0b11110000);
		// instruction is 0b000, which is already set for data[3]...
		data[3] |= (byte)(AddOffset);
		data[2] |= (byte)((byte)AddOffset << 7);
		data[2] |= (byte)((ImmediateOffsetFlag ? 1 : 0) << 6);
		data[2] |= (byte)((WriteBack ? 1 : 0) << 5);
		data[2] |= (byte)((OpCode switch {
			HOpCode.SWP => 0,
			HOpCode.STRH => 0,
			HOpCode.LDRD => 0,
			HOpCode.STRD => 0,
			HOpCode.Reserved => 1,
			HOpCode.LDRH => 1,
			HOpCode.LDRSB => 1,
			HOpCode.LDRSH => 1,
			_ => throw new Exception("Invalid HOpCode")
		}) << 4);
		data[2] |= (byte)(BaseRegister & 0b1111);
		data[1] |= (byte)(DestinationRegister << 4);
		data[1] |= (byte)((ImmediateOffset >> 4) & 0b1111);
		data[0] |= (byte)((Reserved1 ? 1 : 0) << 7);
		data[0] |= (byte)((OpCode switch {
			HOpCode.Reserved => 0,
			HOpCode.STRH => 1,
			HOpCode.LDRH => 1,
			HOpCode.LDRD => 2,
			HOpCode.LDRSB => 2,
			HOpCode.STRD => 3,
			HOpCode.LDRSH => 3,
			_ => throw new Exception("Invalid HOpCode")
		}) << 5);
		data[0] |= (byte)((Reserved2 ? 1 : 0) << 4);

		data[0] |= ImmediateOffsetFlag 
			? (byte)(ImmediateOffset & 0b1111)
			: (byte)(OffsetRegister & 0b1111);
		return data;
	}

}
