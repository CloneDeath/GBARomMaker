using System;
using System.IO;
using System.Linq;
using GBARomMaker.Rom;
using GBARomMaker.Compilation;
using System.Reflection.Metadata;
using System.Reflection.Emit;
using System.Reflection.PortableExecutable;
using System.Reflection.Metadata.Ecma335;

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
		Console.WriteLine(inputAssembly);
		Console.WriteLine(outputRom);

		var OpCodesByValue =
		    typeof(OpCodes)
        	.GetFields()
	        .Where(f => f.FieldType == typeof(OpCode))
	        .Select(f => (OpCode)f.GetValue(null)!)
	        .ToDictionary(op => unchecked((ushort)op.Value));

		using var stream = File.OpenRead(inputAssembly);
		using var peReader = new PEReader(stream);
		var metadata = peReader.GetMetadataReader();

		var entrypoint = DetectEntryPoint(peReader, metadata);

		Console.WriteLine($"Entrypoint: {entrypoint.Namespace}.{entrypoint.Class}.{entrypoint.Name}");
		Console.WriteLine(string.Join(" ", entrypoint.BodyBytes.Select(b => $"0x{b:X2}")));
		PrintCIL(entrypoint.BodyBytes);
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

	public static void PrintCIL(byte[] data) {
		// https://www.ecma-international.org/wp-content/uploads/ECMA-335_6th_edition_june_2012.pdf
		for (var i = 0; i < data.Count(); i++) {
			var op = data[i];
			switch (op) {
				case 0x00:
					Console.WriteLine("nop");
					break;
				case 0x06:
					Console.WriteLine("ldloc.0");
					break;
				case 0x07:
					Console.WriteLine("ldloc.1");
					break;
				case 0x0A:
					Console.WriteLine("stloc.0");
					break;
				case 0x0B:
					Console.WriteLine("stloc.1");
					break;
				case 0x1F: {
					var value = data[i+1];
					i += 1;
					Console.WriteLine($"ldc.i4.s 0x{value:X2}");
					break;
				}
				case 0x20: {
					var args = data[(i+1)..(i+5)];
					var value = BitConverter.ToInt32(args);
					i += 4;
					Console.WriteLine($"ldc.i4 0x{value:X8}");
					break;
				}
				case 0x2A: {
					Console.WriteLine("ret");
					break;
				}
				case 0x53:
					Console.WriteLine("stind.i2");
					break;
				case 0xD3:
					Console.WriteLine("conv.i");
					break;
				default: throw new NotImplementedException($"0x{op:X2}");
			}
		}
	}
}
