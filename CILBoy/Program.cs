using System;
using System.IO;
using System.Linq;
using CILBoy.Rom;
using CILBoy.Compilation;
using CILBoy.CILToArm;
using CILBoy.CIL;

namespace CILBoy;
public static class Program {
	public static int Main(string[] args) {
		if (args.Length < 2) {
			Console.Error.WriteLine("Usage: GbaCompiler input.dll output.gba <args>");
			return 1;
		}

		var showCil = args.Any(a => a == "--show-cil");
		var showArm = args.Any(a => a == "--show-arm");

		var inputAssembly = Path.GetFullPath(args[0]);
		var outputRom = Path.GetFullPath(args[1]);
		
		Console.WriteLine(inputAssembly + " -> " + outputRom);

		using var factory = new CILFactory(Path.GetDirectoryName(inputAssembly) ?? throw new Exception($"Failed to get directory for {inputAssembly}"));
		var assemblyFactory = factory.GetAssemblyFactoryFor(Path.GetFileNameWithoutExtension(inputAssembly));

		var transpiler = new CILToArmTranspiler(showCil);
		var assembly = transpiler.Transpile(assemblyFactory);
		if (showArm) {
			PrintAsm(assembly);
		}

		var newFile = new RomFile();
		newFile.Header.GameTitle = Path.GetFileNameWithoutExtension(args[1]);
		var compiler = new Compiler();
		var machineCode = compiler.GetOperationsForAssembly(assembly);
		if (machineCode.LabelsAreMissing) throw new Exception("Missing labels when compiling to ARM: " + string.Join(", ", machineCode.MissingLabels));
		newFile.Content = machineCode.ToBytes();
		Directory.CreateDirectory(Path.GetDirectoryName(outputRom)!);
		File.WriteAllBytes(outputRom, newFile.ToBytes());
		return 0;
	}

	public static void PrintAsm(string[] instructions) {
		foreach (var line in instructions) {
			if (line.EndsWith(":")) {
				if (line.StartsWith("method_")) Console.WriteLine();
				Console.WriteLine(line);
			}
			else {
				Console.WriteLine($"\t{line}");
			}
		}
	}
}
