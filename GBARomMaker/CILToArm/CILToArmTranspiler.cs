using System;
using System.IO;
using System.Linq;
using System.Reflection.Metadata;
using System.Reflection.Metadata.Ecma335;
using System.Reflection.PortableExecutable;
using GBARomMaker.CILParse;

namespace GBARomMaker.CILToArm;

public record MethodToAssemble(MethodDefinitionRef method);

public class CILToArmTranspiler {
	private PEReader _peReader;
	private MetadataReader _metadata;
	private bool _showCil;

	public CILToArmTranspiler(PEReader peReader, MetadataReader metadata, bool showCil) {
		_peReader = peReader;
		_metadata = metadata;
		_showCil = showCil;
	}

	public string[] Transpile() {
		var assembly = new ARMProgram {
			new ARMLine(0, 0, "ldr sp, =0x03000000 @ CIL stack pointer -- WRAM Internal")
		};

		var entrypoint = DetectEntryPoint();
		ConvertCILToASM(assembly, entrypoint);

		return assembly.GetArm7Assembly();
	}

	private MethodDefinitionRef DetectEntryPoint() {
		var corHeader = _peReader.PEHeaders.CorHeader ?? throw new InvalidDataException("Not a managed assembly.");
		var entryPointToken = corHeader.EntryPointTokenOrRelativeVirtualAddress;
		var entryPointHandle = MetadataTokens.EntityHandle(entryPointToken);
		if (entryPointHandle.Kind != HandleKind.MethodDefinition) throw new InvalidDataException("Entry point is not a managed method.");

		var method = _metadata.GetMethodDefinition((MethodDefinitionHandle)entryPointHandle);
		return new MethodDefinitionRef(_peReader, _metadata, method);
	}

	public void ConvertCILToASM(ARMProgram assembly, MethodDefinitionRef method) {
		var parser = new CILParser();
		var instructions = parser.GetInstructions(method.BodyBytes);

		DeclareMethod(assembly, method);

		if (_showCil) {
			Console.WriteLine($"{method.FullName}");
			Console.WriteLine(string.Join(" ", method.BodyBytes.Select(b => $"0x{b:X2}")));
			PrintCIL(instructions);
			Console.WriteLine();
		}

		foreach (var instruction in instructions) {
			var opcode = instruction.OpCode.Name;
			switch (opcode) {
				case "nop":
				case "conv.i": {
					assembly.Add(instruction.GetBytes().Length, [
						"nop"
					]);
					break;
				}
				case "ldc.i4.m1":
				case "ldc.i4.0":
				case "ldc.i4.1":
				case "ldc.i4.2":
				case "ldc.i4.3":
				case "ldc.i4.4":
				case "ldc.i4.5":
				case "ldc.i4.6":
				case "ldc.i4.7":
				case "ldc.i4.8": {
					var ldc = (GBARomMaker.CILParse.Instructions.LDC_I4_X)instruction;
					assembly.Add(instruction.GetBytes().Length, [
						$"ldr r0, =0x{ldc.Data:X2}",
						"stmia sp!, { r0 }"
					]);
					break;
				}
				case "ldc.i4": {
					var ldc = (GBARomMaker.CILParse.Instructions.LDC_I4)instruction;
					assembly.Add(instruction.GetBytes().Length, [
						$"ldr r0, =0x{ldc.Data:X8}",
						"stmia sp!, { r0 }"
					]);
					break;
				}
				case "ldc.i4.s": {
					var ldc = (GBARomMaker.CILParse.Instructions.LDC_I4_S)instruction;
					assembly.Add(instruction.GetBytes().Length, [
						$"ldr r0, =0x{ldc.Data:X2}",
						"stmia sp!, { r0 }"
					]);
					break;
				}
				case "stloc.0":
				case "stloc.1":
				case "stloc.2":
				case "stloc.3": {
					var location = int.Parse(opcode[6].ToString()); // stloc.X
					var register = location + 9;
					assembly.Add(instruction.GetBytes().Length, [
						$"ldmdb sp!, {{ r{register} }}"
					]);;
					break;
				}
				case "ldloc.0":
				case "ldloc.1":
				case "ldloc.2":
				case "ldloc.3": {
					var location = int.Parse(opcode[6].ToString()); // ldloc.X
					var register = location + 9;
					assembly.Add(instruction.GetBytes().Length, [
						$"stmia sp!, {{ r{register} }}"
					]);
					break;
				}
				case "stind.i2": {
					assembly.Add(instruction.GetBytes().Length, [
						"ldmdb sp!, { r0, r1 }",
						"strh r1, [r0]"
					]);
					break;
				}
				case "ret": {
					assembly.Add(instruction.GetBytes().Length, [
						"ldmdb sp!, { lr }",
						"bx lr"
					]);
					break;
				}
				case "clt": {
					assembly.Add(instruction.GetBytes().Length, [
						"ldmdb sp!, { r0, r1 }",
						"cmp r0, r1",
						"movlt r0, #1",
						"movge r0, #0",
						"stmia sp!, { r0 }"
					]);
					break;
				}
				case "call": {
					HandleCallInstruction(instruction, assembly);
					break;
				}
				case "br.s": {
					var brs = (GBARomMaker.CILParse.Instructions.BR_S)instruction;
					var label = $"jump_{assembly.JumpCount++}";
					assembly.Add(instruction.GetBytes().Length, [
						$"b {label}"
					]);
					var target = assembly.Offset + brs.Target;
					assembly.AddLabel(target, label);
					break;
				}
				case "brtrue.s": {
					var brt = (GBARomMaker.CILParse.Instructions.BRTRUE_S)instruction;
					var label = $"jump_{assembly.JumpCount++}";
					assembly.Add(instruction.GetBytes().Length, [
						"ldmdb sp!, { r0 }",
						"cmp r0, #0",
						$"bne {label}"
					]);
					var target = assembly.Offset + brt.Target;
					assembly.AddLabel(target, label);
					break;
				}
				case "add": {
					assembly.Add(instruction.GetBytes().Length, [
						"ldmdb sp!, { r1, r2 }",
						"add r0,r1,r2",
						"stmia sp!, { r0 }"
					]);
					break;
				}
				case "mul": {
					assembly.Add(instruction.GetBytes().Length, [
						"ldmdb sp!, { r1, r2 }",
						"mul r0,r1,r2",
						"stmia sp!, { r0 }"
					]);
					break;
				}
				default: throw new Exception("Couldn't convert instruction to ARM7 ASM: " + opcode);
			}
		}
		assembly.MethodsTranspiled.Add(method.FullName);
	}

	private void DeclareMethod(ARMProgram assembly, MethodDefinitionRef method) {
		assembly.AddLabel(GetLabelForMethod(method));
	}

	private void HandleCallInstruction(CILInstruction instruction, ARMProgram assembly) {
		var call = (GBARomMaker.CILParse.Instructions.CALL)instruction;
		var handle = MetadataTokens.EntityHandle(call.MetadataToken);
		switch (handle.Kind) {
			case HandleKind.MethodDefinition: {
				var method = _metadata.GetMethodDefinition((MethodDefinitionHandle)handle);

				var methodRef = new MethodDefinitionRef(_peReader, _metadata, method);
				assembly.MethodsToTranspile.Add(methodRef);

				var target = GetLabelForMethod(methodRef);
				assembly.Add(instruction.GetBytes().Length, [
					$"bl {target}"
				]);
				return;
			}
			default: {
				throw new NotImplementedException("Calls to " + handle.Kind + " not yet implemented");
			}
		}
	}

	private string GetLabelForMethod(MethodDefinitionRef method) {
		return $"method_{method.FullName}";
	}
	
	public static void PrintCIL(CILInstruction[] instructions) {
		var offset = 0;
		foreach (var instruction in instructions) {
			Console.WriteLine($"{offset:D4}: {instruction.GetCIL()}");
			offset += instruction.GetBytes().Length;
		}
	}
}
