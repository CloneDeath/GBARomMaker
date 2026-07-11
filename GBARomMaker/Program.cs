using System;
using System.IO;

namespace GBARomMaker;
public static class Program {
	public static void Main() {
		var data = File.ReadAllBytes("../../homebrew/pliko_013b.gba");
		var reader = new RomReader(data);
		var header = reader.Header;
		Console.WriteLine(header.EntryPoint);
		Console.WriteLine(header.GameTitle);
		Console.WriteLine(header.GameCode);
		Console.WriteLine(header.MakerCode);

	}

}
