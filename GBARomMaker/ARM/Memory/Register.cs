using System;

namespace GBARomMaker.ARM.Memory;

public class Register : IOffset {
	public bool IsImmediate => false;

	public Register() {
		OpRegister = 0;
	}

	public Register(byte register) {
		OpRegister = register;
	}

	public byte OpRegister { get; set; }

	public byte[] ToBytes() {
		var data = new byte[2] { 0, 0 };
		throw new NotImplementedException();
	}
}
