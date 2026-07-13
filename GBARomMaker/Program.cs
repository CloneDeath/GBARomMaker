using System;
using System.IO;
using System.Linq;
using GBARomMaker.Rom;
using GBARomMaker.Compilation;

namespace GBARomMaker;
public static class Program {
	public static int Main(string[] args) {
		if (args.Length != 2)
		{
			Console.Error.WriteLine("Usage: GbaCompiler <input.dll> <output.gba>");
			return 1;
		}

		string inputAssembly = Path.GetFullPath(args[0]);
		string outputRom = Path.GetFullPath(args[1]);

		var newFile = new RomFile();
		newFile.Header.GameTitle = "red pixel";

		var compiler = new Compiler();

		var assembly = new string[]{
			"ldr r0, =0x04000000     @ Display control register",
			"ldr r1, =0x0403         @ Mode 3 + BG2 enabled",
			"strh r1, [r0]",
			"ldr r0, =0x06000000     @ Top-left pixel in VRAM",
			"mov r1, #0x1F           @ Red",
			"strh r1, [r0]"
		};

		newFile.Content = assembly.SelectMany(a => compiler.GetOperationForLine(a))
			.SelectMany(op => op.ToBytes())
			.ToArray();
		
		Directory.CreateDirectory(Path.GetDirectoryName(outputRom)!);
		File.WriteAllBytes(outputRom, newFile.ToBytes());
		Console.WriteLine("Hello Red");
		Console.WriteLine(inputAssembly);
		Console.WriteLine(outputRom);
		return 0;
	}
}
