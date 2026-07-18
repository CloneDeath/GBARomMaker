using System.Text;
using System.Linq;
using GBARomMaker.ARM;
using System;

namespace GBARomMaker.Rom;

public class Header {
	public Header() {
		this.EntryPoint = new Branch {
			Offset = 0xC0
		};
		this.NintendoLogo = new byte[156] {
			0x24, 0xFF, 0xAE, 0x51, 0x69, 0x9A, 0xA2, 0x21, 0x3D, 0x84, 0x82, 0x0A, 0x84, 0xE4, 0x09, 0xAD,
			0x11, 0x24, 0x8B, 0x98, 0xC0, 0x81, 0x7F, 0x21, 0xA3, 0x52, 0xBE, 0x19, 0x93, 0x09, 0xCE, 0x20,
			0x10, 0x46, 0x4A, 0x4A, 0xF8, 0x27, 0x31, 0xEC, 0x58, 0xC7, 0xE8, 0x33, 0x82, 0xE3, 0xCE, 0xBF,
			0x85, 0xF4, 0xDF, 0x94, 0xCE, 0x4B, 0x09, 0xC1, 0x94, 0x56, 0x8A, 0xC0, 0x13, 0x72, 0xA7, 0xFC,
			0x9F, 0x84, 0x4D, 0x73, 0xA3, 0xCA, 0x9A, 0x61, 0x58, 0x97, 0xA3, 0x27, 0xFC, 0x03, 0x98, 0x76,
			0x23, 0x1D, 0xC7, 0x61, 0x03, 0x04, 0xAE, 0x56, 0xBF, 0x38, 0x84, 0x00, 0x40, 0xA7, 0x0E, 0xFD,
			0xFF, 0x52, 0xFE, 0x03, 0x6F, 0x95, 0x30, 0xF1, 0x97, 0xFB, 0xC0, 0x85, 0x60, 0xD6, 0x80, 0x25,
			0xA9, 0x63, 0xBE, 0x03, 0x01, 0x4E, 0x38, 0xE2, 0xF9, 0xA2, 0x34, 0xFF, 0xBB, 0x3E, 0x03, 0x44,
			0x78, 0x00, 0x90, 0xCB, 0x88, 0x11, 0x3A, 0x94, 0x65, 0xC0, 0x7C, 0x63, 0x87, 0xF0, 0x3C, 0xAF,
			0xD6, 0x25, 0xE4, 0x8B, 0x38, 0x0A, 0xAC, 0x72, 0x21, 0xD4, 0xF8, 0x07
		};
		this.GameTitle = "Unknown";
		this.GameCode = "";
		this.MakerCode = "";
		this.FixedValue = 0x96;
		this.MainUnitCode = 0;
		this.DeviceType = 0;
		this.ReservedArea1 = new byte[7] { 0, 0, 0, 0, 0, 0, 0 };
		this.SoftwareVersion = 0;
		this.ComplementCheck = this.CalculateComplementCheck();
		this.ReservedArea2 = new byte[2] { 0, 0 };
	}

	public Header(byte[] data) {
		this.EntryPoint = new Branch(data[0..4]);
		this.NintendoLogo = data[0x4..(0x4 + 156)];
		this.GameTitle = Encoding.ASCII.GetString(data[0xA0..(0xA0 + 12)]);
		this.GameCode = Encoding.ASCII.GetString(data[0xAC..(0xAC + 4)]);
		this.MakerCode = Encoding.ASCII.GetString(data[0xB0..(0xB0 + 2)]);
		this.FixedValue = data[0xB2];
		this.MainUnitCode = data[0xB3];
		this.DeviceType = data[0xB4];
		this.ReservedArea1 = data[0xB5..(0xB5 + 7)];
		this.SoftwareVersion = data[0xBC];
		this.ComplementCheck = data[0xBD];
		this.ReservedArea2 = data[0xBE..(0xBE + 2)];
	}

	public IInstruction EntryPoint { get; set; }
	public byte[] NintendoLogo { get; set; }
	public string GameTitle { get; set; }
	public string GameCode { get; set; }
	public string MakerCode { get; set; }
	public byte FixedValue { get; set; } = 0x96;
	public byte MainUnitCode { get; set; }
	public byte DeviceType { get; set; }
	public byte[] ReservedArea1 { get; set; }
	public byte SoftwareVersion { get; set; }
	public byte ComplementCheck { get; set; }
	public byte[] ReservedArea2 { get; set; }

	public byte[] ToBytes(bool calculateChecksum = true) {
		byte[] titleBytes = new byte[12];
		Encoding.ASCII.GetBytes(this.GameTitle.AsSpan(0, Math.Min(this.GameTitle.Length, 12)), titleBytes);
		
		byte[] gameCodeBytes = new byte[4];
		Encoding.ASCII.GetBytes(this.GameCode.AsSpan(0, Math.Min(this.GameCode.Length, 4)), gameCodeBytes);
		
		byte[] makerCodeBytes = new byte[2];
		Encoding.ASCII.GetBytes(this.MakerCode.AsSpan(0, Math.Min(this.MakerCode.Length, 2)), makerCodeBytes);

		if (this.NintendoLogo.Length != 156) throw new Exception("Nintendo Logo must be exactly 156 bytes long. Got " + this.NintendoLogo.Length);
		if (this.ReservedArea1.Length != 7) throw new Exception("Reserved Area 1 must be exactly 7 bytes long. Got " + this.ReservedArea1.Length);
		if (this.ReservedArea2.Length != 2) throw new Exception("Reserved Area 2 must be exactly 2 bytes long. Got " + this.ReservedArea2.Length);

		var result = this.EntryPoint.ToBytes()
			.Concat(this.NintendoLogo)
			.Concat(titleBytes)
			.Concat(gameCodeBytes)
			.Concat(makerCodeBytes)
			.Concat([FixedValue])
			.Concat([MainUnitCode])
			.Concat([DeviceType])
			.Concat(ReservedArea1)
			.Concat([SoftwareVersion])
			.Concat([ calculateChecksum ? CalculateComplementCheck() : ComplementCheck])
			.Concat(ReservedArea2)
			.ToArray();

		if (result.Length != 192) throw new Exception("Failed to properly construct header to bytes. Expected length 192, got " + result.Length);

		return result;
	}

	public byte CalculateComplementCheck() {
		var complement = 0;

		byte[] titleBytes = new byte[12];
		Encoding.ASCII.GetBytes(this.GameTitle.AsSpan(0, Math.Min(this.GameTitle.Length, 12)), titleBytes);
		
		byte[] gameCodeBytes = new byte[4];
		Encoding.ASCII.GetBytes(this.GameCode.AsSpan(0, Math.Min(this.GameCode.Length, 4)), gameCodeBytes);
		
		byte[] makerCodeBytes = new byte[2];
		Encoding.ASCII.GetBytes(this.MakerCode.AsSpan(0, Math.Min(this.MakerCode.Length, 2)), makerCodeBytes);

		var result = titleBytes
			.Concat(gameCodeBytes)
			.Concat(makerCodeBytes)
			.Concat([FixedValue])
			.Concat([MainUnitCode])
			.Concat([DeviceType])
			.Concat(ReservedArea1)
			.Concat([SoftwareVersion])
			.ToArray();

		foreach (var value in result) {
			complement -= value;
		}
		complement -= 0x19;
		return (byte)(complement & 0xFF);
	}
}
