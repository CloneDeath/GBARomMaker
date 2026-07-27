using System;
using System.IO;
using System.Linq;
using System.Reflection.Metadata;
using System.Reflection.Metadata.Ecma335;
using System.Reflection.PortableExecutable;
using GBARomMaker.CIL;
using GBARomMaker.CILParse;

namespace GBARomMaker.CILToArm;

public record MethodToAssemble(CILMethodDefinition method);

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
			new ARMLine(-1, 0, "ldr sp, =0x03008000 @ CIL stack pointer -- WRAM Internal"),
		};

		var entrypoint = DetectEntryPoint();
		ConvertCILToASM(assembly, entrypoint);

		while (assembly.MethodsToTranspile.Any()) {
			var method = assembly.MethodsToTranspile.Dequeue();
			ConvertCILToASM(assembly, method);
		}
		
		assembly.Add(new ARMLine(-1, 1, $"ldr r8, =0x{assembly.HeapStart:X8} @ Heap Start -- WRAM External"));
		return assembly.GetArm7Assembly();
	}

	private CILMethodDefinition DetectEntryPoint() {
		var corHeader = _peReader.PEHeaders.CorHeader ?? throw new InvalidDataException("Not a managed assembly.");
		var entryPointToken = corHeader.EntryPointTokenOrRelativeVirtualAddress;
		var entryPointHandle = MetadataTokens.EntityHandle(entryPointToken);
		if (entryPointHandle.Kind != HandleKind.MethodDefinition) throw new InvalidDataException("Entry point is not a managed method.");

		var method = _metadata.GetMethodDefinition((MethodDefinitionHandle)entryPointHandle);
		return new CILMethodDefinition(_peReader, _metadata, method);
	}

	public void ConvertCILToASM(ARMProgram assembly, ICILMethod method) {
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
		// Free Register 4 = r3
		// Free Register 5 = r4
		// Function Stack  = r7
		// Heap Pointer    = r8 <- Temporary until we implement malloc/free
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
				case "conv.u1": {
					assembly.Add(instruction.GetBytes().Length, [
						"pop sp!, { r1 }",
						"ldr r2, =0xFF",
						"and r0, r1, r2",
						"push sp!, { r0 }"
					]);
					break;
				}
				case "conv.u2": {
					assembly.Add(instruction.GetBytes().Length, [
						"pop sp!, { r1 }",
						"ldr r2, =0xFFFF",
						"and r0, r1, r2",
						"push sp!, { r0 }"
					]);
					break;
				}
				case "dup": {
					assembly.Add(instruction.GetBytes().Length, [
						"ldr r0, [sp]",
						"push sp!, { r0 }"
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
					var argCount = method.ParameterCount + (method.IsInstance ? 1 : 0);
					var wordsBack = (argCount - ldarg.Argument) - 1;
					assembly.Add(instruction.GetBytes().Length, [
						$"ldr r0, [r7, #{wordsBack * 4}] @ arg {ldarg.Argument}",
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
						$"pop sp!, {{ r{register} }} @ local {location}"
					]);;
					break;
				}
				case "stloc.s": {
					var stlocs = (GBARomMaker.CILParse.Instructions.STLOC_S)instruction;
					var location = stlocs.Location;
					if (location <= 3) {
						var register = location + 9;
						assembly.Add(instruction.GetBytes().Length, [
							$"pop sp!, {{ r{register} }} @ local {location}"
						]);
						break;
					}
					var offset = (location - 4) * 4;
					assembly.Add(instruction.GetBytes().Length, [
						"ldr r0, =0x03000000",
						"pop sp!, { r1 }",
						$"str r1, [r0, #{offset}] @ local {location}",
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
						$"push sp!, {{ r{register} }} @ local {location}"
					]);
					break;
				}
				case "ldloc.s": {
					var ldlocs = (GBARomMaker.CILParse.Instructions.LDLOC_S)instruction;
					var location = ldlocs.Location;
					if (location <= 3) {
						var register = location + 9;
						assembly.Add(instruction.GetBytes().Length, [
							$"push sp!, {{ r{register} }} @ local {location}"
						]);
						break;
					}
					var offset = (location - 4) * 4;
					assembly.Add(instruction.GetBytes().Length, [
						"ldr r0, =0x03000000",
						$"ldr r1, [r0, #{offset}] @ local {location}",
						"push sp!, { r1 }"
					]);;
					break;
				}
				case "ldind.u2": {
					assembly.Add(instruction.GetBytes().Length, [
						"pop sp!, { r0 }",
						"ldrh r1, [r0]",
						"push sp!, { r1 }",
					]);
					break;
				}
				case "stind.i1": {
					// If we're not in byte-addressable memory, then read-modify-write a short instead
					var end = "byte_store_" + assembly.JumpCount++;
					assembly.Add(instruction.GetBytes().Length, [
						"pop sp!, { r0, r1 } @ value, addr",
						"ldr r2, =0x05000000 @ VRAM Start",
						"cmp r1, r2",
						"strltb r0, [r1]",
						$"blt {end}",
						"ldr r2, =0x08000000 @ VRAM End",
						"cmp r1, r2",
						"strgeb r0, [r1]",
						$"bge {end}",
						"ldr r2, =0x01",
						"and r4, r1, r2 @ byte offset",
						"mvn r2, r2",
						"and r3, r1, r2 @ half address",

						"ldrh r2, [r3]",
						"cmp r4, #0",
						"ldreq r1, =0xFF00",
						"ldrne r1, =0x00FF",
						"and r2, r2, r1",
						"ldreq r1, =0",
						"ldrne r1, =8",
						"lsl r0, r0, r1",
						"orr r2, r0, r2",
						"strh r2, [r3]",
						$"{end}:",
					]);
					break;
				}
				case "stind.i2": {
					assembly.Add(instruction.GetBytes().Length, [
						"pop sp!, { r0, r1 } @ value, addr",
						"strh r0, [r1]"
					]);
					break;
				}
				case "ldfld": {
					HandleLoadFieldInstruction(instruction, assembly);
					break;
				}
				case "ldsfld": {
					HandleLoadStaticFieldInstruction(instruction, assembly);
					break;
				}
				case "stfld": {
					HandleStoreFieldInstruction(instruction, assembly);
					break;
				}
				case "stsfld": {
					HandleStoreStaticFieldInstruction(instruction, assembly);
					break;
				}
				case "ret": {
					assembly.Add(instruction.GetBytes().Length, [
						$"sub sp, r7, #{11 * 4}",
						"pop sp!, { r0, r1, r2, r3, r4, r7, r9, r10, r11, r12, lr }",
					]);
					// pop any method parameters
					if (method.ParameterCount > 0 || method.IsInstance) {
						var argsToPop = (method.IsInstance ? 1 : 0) + method.ParameterCount;
						assembly.Add(0, [
							$"add sp, sp, #{argsToPop * 4}"
						]);
					}
					assembly.Add(0, [
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
						"cmp r1, r0",
						"movgt r0, #1",
						"movle r0, #0",
						"push sp!, { r0 }"
					]);
					break;
				}
				case "clt": {
					assembly.Add(instruction.GetBytes().Length, [
						"pop sp!, { r0, r1 }",
						"cmp r1, r0",
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
				case "brfalse.s": {
					var brt = (GBARomMaker.CILParse.Instructions.BRFALSE_S)instruction;
					var label = $"jump_{assembly.JumpCount++}";
					assembly.Add(instruction.GetBytes().Length, [
						"pop sp!, { r0 }",
						"cmp r0, #0",
						$"beq {label}"
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
				case "and": {
					assembly.Add(instruction.GetBytes().Length, [
						"pop sp!, { r1, r2 }",
						"and r0, r1, r2",
						"push sp!, { r0 }"
					]);
					break;
				}
				case "or": {
					assembly.Add(instruction.GetBytes().Length, [
						"pop sp!, { r1, r2 }",
						"orr r0, r1, r2",
						"push sp!, { r0 }"
					]);
					break;
				}
				case "mul": {
					assembly.Add(instruction.GetBytes().Length, [
						"pop sp!, { r1, r2 }",
						"mul r0, r1, r2",
						"push sp!, { r0 }"
					]);
					break;
				}
				case "shl": {
					assembly.Add(instruction.GetBytes().Length, [
						"pop sp!, { r0, r1 } @ shiftAmount, value",
						"lsl r2, r1, r0",
						"push sp!, { r2 }"
					]);
					break;
				}
				default: throw new Exception("Couldn't convert instruction to ARM7 ASM: " + opcode);
			}
		}
		assembly.MethodsTranspiled.Add(method.FullName);
	}

	private void DeclareMethod(ARMProgram assembly, ICILMethod method) {
		assembly.Add(0, [
			$"{GetLabelForMethod(method)}:",
			"push sp!, { r0, r1, r2, r3, r4, r7, r9, r10, r11, r12, lr }",
			$"add r7, sp, #{11 * 4}"
		]);
	}

	private void HandleLoadFieldInstruction(CILInstruction instruction, ARMProgram assembly) {
		var ldfld = (GBARomMaker.CILParse.Instructions.LDFLD)instruction;
		var cilFactory = new CILFactory(_peReader, _metadata);
		var field = cilFactory.GetFieldDefinition(ldfld.MetadataToken);
		var classLayout = assembly.GetClassLayout(field.Parent);
		assembly.Add(instruction.GetBytes().Length, [
			$"pop sp!, {{ r0 }} @ {classLayout.FullName}",
			$"ldr r1, [r0, #{classLayout.GetFieldOffset(field)}] @ {field.Name}",
			"push sp!, { r1 }"
		]);
	}
	
	private void HandleLoadStaticFieldInstruction(CILInstruction instruction, ARMProgram assembly) {
		var ldsfld = (GBARomMaker.CILParse.Instructions.LDSFLD)instruction;
		var cilFactory = new CILFactory(_peReader, _metadata);
		var field = cilFactory.GetFieldDefinition(ldsfld.MetadataToken);
		
		var staticClass = assembly.GetStaticClassLayout(field.Parent);
		var constructor = staticClass.Constructor;
		assembly.Add(instruction.GetBytes().Length, [
			$"ldr r0, =0x{staticClass.StartAddress:X8} @ static ${staticClass.FullName}",
		]);
		if (constructor != null) {
			assembly.MethodsToTranspile.Enqueue(constructor);
			assembly.Add(0, [
				$"ldr r1, [r0]",
				$"cmp r1, #1",
				$"ldrne r1, =1",
				$"strne r1, [r0]",
				$"blne {GetLabelForMethod(constructor)}",
			]);
		}
		assembly.Add(0, [
		 	$"ldr r1, [r0, #{staticClass.GetFieldOffset(field)}] @ {field.Name}",
		 	"push sp!, { r1 }"
		]);
	}

	private void HandleStoreFieldInstruction(CILInstruction instruction, ARMProgram assembly) {
		var stfld = (GBARomMaker.CILParse.Instructions.STFLD)instruction;
		var cilFactory = new CILFactory(_peReader, _metadata);
		var field = cilFactory.GetFieldDefinition(stfld.MetadataToken);

		var classLayout = assembly.GetClassLayout(field.Parent);
		assembly.Add(instruction.GetBytes().Length, [
			"pop sp!, { r0, r1 } @ value, obj",
			$"str r0, [r1, #{classLayout.GetFieldOffset(field)}] @ {field.FullName}"
		]);
	}

	private void HandleStoreStaticFieldInstruction(CILInstruction instruction, ARMProgram assembly) {
		var stsfld = (GBARomMaker.CILParse.Instructions.STSFLD)instruction;
		var cilFactory = new CILFactory(_peReader, _metadata);
		var field = cilFactory.GetFieldDefinition(stsfld.MetadataToken);

		var staticClass = assembly.GetStaticClassLayout(field.Parent);
		var constructor = staticClass.Constructor;
		assembly.Add(instruction.GetBytes().Length, [
			$"ldr r0, =0x{staticClass.StartAddress:X8} @ static ${staticClass.FullName}",
		]);
		if (constructor != null) {
			assembly.MethodsToTranspile.Enqueue(constructor);
			assembly.Add(0, [
				$"ldr r1, [r0]",
				$"cmp r1, #1",
				$"ldrne r1, =1",
				$"strne r1, [r0]",
				$"blne {GetLabelForMethod(constructor)}",
			]);
		}
		assembly.Add(0, [
		 	"pop sp!, { r1 }",
		 	$"str r1, [r0, #{staticClass.GetFieldOffset(field)}] @ {field.Name}",
		]);
	}

	private void HandleCallInstruction(CILInstruction instruction, ARMProgram assembly) {
		var call = (GBARomMaker.CILParse.Instructions.CALL)instruction;
		var cilFactory = new CILFactory(_peReader, _metadata);
		var method = cilFactory.GetMethodDefinition(call.MetadataToken);

		if (method.FullName == "System.Object..ctor") {
			assembly.Add(instruction.GetBytes().Length, [
				$"add sp, sp, #4 @ Pop `this`; Calling '{method.FullName}'"
			]);
			return;
		}
		
		assembly.MethodsToTranspile.Enqueue(method);
		var target = GetLabelForMethod(method);
		assembly.Add(instruction.GetBytes().Length, [
			$"bl {target}"
		]);
	}

	private void HandleNewObjInstruction(CILInstruction instruction, ARMProgram assembly) {
		var newobj = (GBARomMaker.CILParse.Instructions.NEWOBJ)instruction;
		var handle = MetadataTokens.EntityHandle(newobj.MetadataToken);
		switch (handle.Kind) {
			case HandleKind.MethodDefinition: {
				var method = _metadata.GetMethodDefinition((MethodDefinitionHandle)handle);
				var methodRef = new CILMethodDefinition(_peReader, _metadata, method);
				if (methodRef.Name != ".ctor") {
					throw new Exception("Tried to initialize an object with something that isn't a contructor: " + methodRef.FullName);
				}

				var classLayout = assembly.GetClassLayout(methodRef.Parent);

				var target = GetLabelForMethod(methodRef);
				assembly.Add(instruction.GetBytes().Length, [
					"push sp!, { r8 } @ push object ref onto the stack...",
					"push sp!, { r8 } @ push it again for the 'this' param of the constructor",
					$"add r8, r8, #{classLayout.Size}",
					$"bl {target}"
				]);

				assembly.MethodsToTranspile.Enqueue(methodRef);
				return;
			}
			default: {
				throw new NotImplementedException($"New Objects for {handle.Kind} constructors not yet implemented");
			}
		}
	}

	private string GetLabelForMethod(ICILMethod method) {
		return $"method_{method.FullName}".Replace(".", "_").Replace("<", "_").Replace(">", "_").Replace("$", "_");
	}
	
	public void PrintCIL(CILInstruction[] instructions) {
		var factory = new CILFactory(_peReader, _metadata);
		var offset = 0;
		foreach (var instruction in instructions) {
			Console.WriteLine($"{offset:D4}: {instruction.GetCIL(factory)}");
			offset += instruction.GetBytes().Length;
		}
	}
}
