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
			new ARMLine(-1, 0, "ldr sp, =0x03008000 @ CIL stack pointer -- WRAM Internal")
		};

		var entrypoint = DetectEntryPoint();
		ConvertCILToASM(assembly, entrypoint);

		while (assembly.MethodsToTranspile.Any()) {
			var method = assembly.MethodsToTranspile.Dequeue();
			ConvertCILToASM(assembly, method);
		}

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
		if (assembly.MethodsTranspiled.Contains(method.FullName)) return;

		var parser = new CILParser();
		var instructions = parser.GetInstructions(method.BodyBytes);

		DeclareMethod(assembly, method);

		if (_showCil) {
			Console.WriteLine($"{method.FullName}");
			Console.WriteLine(string.Join(" ", method.BodyBytes.Select(b => $"0x{b:X2}")));
			PrintCIL(instructions);
			Console.WriteLine();
		}

		// Free Register 1 = r0
		// Free Register 2 = r1
		// Free Register 3 = r2
		// Function Stack  = r3
		// Local 0         = r9
		// Local 1         = r10
		// Local 2         = r11
		// Local 3         = r12
		// Stack Pointer   = sp/r13
		// Link Register   = lr/r14
		// Program Counter = pc/r15

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
						"push sp!, { r0 }"
					]);
					break;
				}
				case "ldc.i4": {
					var ldc = (GBARomMaker.CILParse.Instructions.LDC_I4)instruction;
					assembly.Add(instruction.GetBytes().Length, [
						$"ldr r0, =0x{ldc.Data:X8}",
						"push sp!, { r0 }"
					]);
					break;
				}
				case "ldc.i4.s": {
					var ldc = (GBARomMaker.CILParse.Instructions.LDC_I4_S)instruction;
					assembly.Add(instruction.GetBytes().Length, [
						$"ldr r0, =0x{ldc.Data:X2}",
						"push sp!, { r0 }"
					]);
					break;
				}
				case "ldarg.0":
				case "ldarg.1":
				case "ldarg.2":
				case "ldarg.3": {
					var ldarg = (GBARomMaker.CILParse.Instructions.LDARG)instruction;
					assembly.Add(instruction.GetBytes().Length, [
						$"ldr r0, [r3, #-{(method.ArgumentCount - ldarg.Argument) * 4}]",
						"push sp!, { r0 }"
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
						$"pop sp!, {{ r{register} }}"
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
						$"push sp!, {{ r{register} }}"
					]);
					break;
				}
				case "stind.i2": {
					assembly.Add(instruction.GetBytes().Length, [
						"pop sp!, { r0, r1 }",
						"strh r0, [r1]"
					]);
					break;
				}
				case "ret": {
					assembly.Add(instruction.GetBytes().Length, [
						$"sub sp, r3, #{9 * 4}",
						"pop sp!, { r0, r1, r2, r3, r9, r10, r11, r12, lr }",
						"bx lr"
					]);
					break;
				}
				case "ceq": {
					assembly.Add(instruction.GetBytes().Length, [
						"pop sp!, { r0, r1 }",
						"cmp r0, r1",
						"moveq r0, #1",
						"movne r0, #0",
						"push sp!, { r0 }"
					]);
					break;
				}
				case "cgt": {
					assembly.Add(instruction.GetBytes().Length, [
						"pop sp!, { r0, r1 }",
						"cmp r0, r1",
						"movgt r0, #1",
						"movle r0, #0",
						"push sp!, { r0 }"
					]);
					break;
				}
				case "clt": {
					assembly.Add(instruction.GetBytes().Length, [
						"pop sp!, { r0, r1 }",
						"cmp r0, r1",
						"movlt r0, #1",
						"movge r0, #0",
						"push sp!, { r0 }"
					]);
					break;
				}
				case "call": {
					HandleCallInstruction(instruction, assembly);
					break;
				}
				case "newobj": {
					HandleNewObjInstruction(instruction, assembly);
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
						"pop sp!, { r0 }",
						"cmp r0, #0",
						$"bne {label}"
					]);
					var target = assembly.Offset + brt.Target;
					assembly.AddLabel(target, label);
					break;
				}
				case "add": {
					assembly.Add(instruction.GetBytes().Length, [
						"pop sp!, { r1, r2 }",
						"add r0,r1,r2",
						"push sp!, { r0 }"
					]);
					break;
				}
				case "mul": {
					assembly.Add(instruction.GetBytes().Length, [
						"pop sp!, { r1, r2 }",
						"mul r0,r1,r2",
						"push sp!, { r0 }"
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
		assembly.Add(0, [
			"push sp!, { r0, r1, r2, r3, r9, r10, r11, r12, lr }",
			$"add r3, sp, #{9 * 4}"
		]);
	}

	private void HandleCallInstruction(CILInstruction instruction, ARMProgram assembly) {
		var call = (GBARomMaker.CILParse.Instructions.CALL)instruction;
		var handle = MetadataTokens.EntityHandle(call.MetadataToken);
		switch (handle.Kind) {
			case HandleKind.MethodDefinition: {
				var method = _metadata.GetMethodDefinition((MethodDefinitionHandle)handle);

				var methodRef = new MethodDefinitionRef(_peReader, _metadata, method);
				assembly.MethodsToTranspile.Enqueue(methodRef);

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

	private void HandleNewObjInstruction(CILInstruction instruction, ARMProgram assembly) {
		var newobj = (GBARomMaker.CILParse.Instructions.NEWOBJ)instruction;
		var handle = MetadataTokens.EntityHandle(newobj.MetadataToken);
		switch (handle.Kind) {
			case HandleKind.MethodDefinition: {
				var method = _metadata.GetMethodDefinition((MethodDefinitionHandle)handle);
				var methodRef = new MethodDefinitionRef(_peReader, _metadata, method);
				if (methodRef.Name != ".ctor") {
					throw new Exception("Tried to initialize an object with something that isn't a contructor: " + methodRef.FullName);
				}

				assembly.MethodsToTranspile.Enqueue(methodRef);
				throw new Exception("Not done");
			}
			default: {
				throw new NotImplementedException($"New Objects for {handle.Kind} constructors not yet implemented");
			}
		}
	}

	private string GetLabelForMethod(MethodDefinitionRef method) {
		return $"method_{method.FullName}".Replace(".", "_").Replace("<", "_").Replace(">", "_").Replace("$", "_");
	}
	
	public static void PrintCIL(CILInstruction[] instructions) {
		var offset = 0;
		foreach (var instruction in instructions) {
			Console.WriteLine($"{offset:D4}: {instruction.GetCIL()}");
			offset += instruction.GetBytes().Length;
		}
	}
}
