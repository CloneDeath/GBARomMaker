using System;
using System.IO;
using System.Linq;
using GBARomMaker.Rom;
using GBARomMaker.Compilation;
using System.Reflection.Metadata;
using System.Reflection.PortableExecutable;
using System.Reflection.Metadata.Ecma335;
using GBARomMaker.CILParse;

namespace GBARomMaker;
public static class Program {
	public static int Main(string[] args) {
		if (args.Length != 2)
		{
			Console.Error.WriteLine("Usage: GbaCompiler <input.dll> <output.gba>");
			return 1;
		}

		var inputAssembly = Path.GetFullPath(args[0]);
		var outputRom = Path.GetFullPath(args[1]);
		
		Console.WriteLine(inputAssembly);
		Console.WriteLine(outputRom);

		using var stream = File.OpenRead(inputAssembly);
		using var peReader = new PEReader(stream);
		var metadata = peReader.GetMetadataReader();

		var entrypoint = DetectEntryPoint(peReader, metadata);

		Console.WriteLine($"Entrypoint: {entrypoint.Namespace}.{entrypoint.Class}.{entrypoint.Name}");
		Console.WriteLine(string.Join(" ", entrypoint.BodyBytes.Select(b => $"0x{b:X2}")));

		var parser = new CILParser();
		var instructions = parser.GetInstructions(entrypoint.BodyBytes);
		PrintCIL(instructions);
		var assembly = CILToArm.CILToArm.Translate(instructions);

		var newFile = new RomFile();
		newFile.Header.GameTitle = "red pixel";
		var compiler = new Compiler();
		newFile.Content = assembly.SelectMany(a => compiler.GetOperationForLine(a))
			.SelectMany(op => op.ToBytes())
			.ToArray();
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

	public static MethodDefinitionRef DetectEntryPoint(PEReader peReader, MetadataReader metadata) {
		var corHeader = peReader.PEHeaders.CorHeader ?? throw new InvalidDataException("Not a managed assembly.");
		var entryPointToken = corHeader.EntryPointTokenOrRelativeVirtualAddress;
		var entryPointHandle = MetadataTokens.EntityHandle(entryPointToken);
		if (entryPointHandle.Kind != HandleKind.MethodDefinition) throw new InvalidDataException("Entry point is not a managed method.");

		var methodHandle = (MethodDefinitionHandle)entryPointHandle;
		var method = metadata.GetMethodDefinition(methodHandle);
		return new MethodDefinitionRef(peReader, metadata, method);
	}

	public static void PrintCIL(CILInstruction[] instructions) {
		var offset = 0;
		foreach (var instruction in instructions) {
			Console.WriteLine($"{offset:D4}: {instruction.GetCIL()}");
			offset += instruction.GetBytes().Length;
		}
	}
}
