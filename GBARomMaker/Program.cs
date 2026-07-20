using System;
using System.IO;
using System.Linq;
using GBARomMaker.Rom;
using GBARomMaker.Compilation;
using System.Reflection.Metadata;
using System.Reflection.PortableExecutable;
using GBARomMaker.CILToArm;

namespace GBARomMaker;
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

		using var stream = File.OpenRead(inputAssembly);
		using var peReader = new PEReader(stream);
		var metadata = peReader.GetMetadataReader();

		var transpiler = new CILToArmTranspiler(peReader, metadata, showCil);
		var assembly = transpiler.Transpile();
		if (showArm) {
			PrintAsm(assembly);
		}

		var newFile = new RomFile();
		newFile.Header.GameTitle = "red pixel";
		var compiler = new Compiler();
		var machineCode = compiler.GetOperationsForAssembly(assembly);
		if (machineCode.LabelsAreMissing) throw new Exception("Missing labels when compiling to ARM: " + string.Join(", ", machineCode.MissingLabels));
		newFile.Content = machineCode.ToBytes();
		Directory.CreateDirectory(Path.GetDirectoryName(outputRom)!);
		File.WriteAllBytes(outputRom, newFile.ToBytes());

		//foreach (TypeDefinitionHandle typeHandle in metadata.TypeDefinitions)
		//{
		//	TypeDefinition type = metadata.GetTypeDefinition(typeHandle);

		//	var namespaceName = metadata.GetString(type.Namespace);
		//	var typeName = metadata.GetString(type.Name);

		//	Console.WriteLine($"Type: {namespaceName}.{typeName}");
		//	foreach (MethodDefinitionHandle methodHandle in type.GetMethods())
		//	{
		//		MethodDefinition method = metadata.GetMethodDefinition(methodHandle);
		//		var methodName = metadata.GetString(method.Name);
		//		Console.WriteLine($"  Method: {methodName}");

		//		if (method.RelativeVirtualAddress == 0)
		//			continue; // Abstract, extern, etc.

		//		MethodBodyBlock body = peReader.GetMethodBody(method.RelativeVirtualAddress);

		//		byte[] ilBytes = body.GetILBytes().ToArray();

		//		Console.WriteLine($"    IL: {string.Join(" ", ilBytes.Select(b => $"{b:X2}"))}");
		//	}
		//}

		return 0;
	}

	public static void PrintAsm(string[] instructions) {
		foreach (var line in instructions) {
			Console.WriteLine(line);
		}
	}
}
