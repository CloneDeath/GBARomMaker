using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection.Metadata;
using System.Reflection.Metadata.Ecma335;
using CILBoy.CIL;
using CILBoy.CILParse;
using CILBoy.CILToArm.CallHandlers;
using CILBoy.CILToArm.ControlFlow;
using CILBoy.CILToArm.Handlers;

namespace CILBoy.CILToArm;

public record MethodToAssemble(CILMethodDefinition method);

public class CILToArmTranspiler {
	private readonly CILAssemblyFactory _factory;
	private readonly bool _showCil;

	public CILToArmTranspiler(CILAssemblyFactory factory, bool showCil) {
		_factory = factory;
		_showCil = showCil;
	}

	public string[] Transpile() {
		var entrypoint = DetectEntryPoint();

		var header_line = 2;
		var assembly = new ARMProgram {
			new ARMLine(-1, 0, "ldr sp, =0x03007F00 @ CIL stack pointer -- WRAM Internal"),
			// heap start (added below...)
			new ARMLine(-1, header_line++, "ldr r0, =gba_irq_handler @ Install IRQ Handler"),
			new ARMLine(-1, header_line++, "ldr r1, =0x03007FFC"),
			new ARMLine(-1, header_line++, "str r0, [r1]"),
			new ARMLine(-1, header_line++, "ldr r0, =0x04FFF780 @ Enable mGBA logs"),
			new ARMLine(-1, header_line++, "ldr r1, =0xC0DE"),
			new ARMLine(-1, header_line++, "strh r1, [r0]"),
			new ARMLine(-1, header_line++, $"b {GetLabelForMethod(entrypoint)}"),
		};
		foreach (var line in AsmFunctions.GetIRQHandler()) {
			assembly.Add(new ARMLine(-1, header_line++, line));
		}

		ConvertCILToASM(assembly, entrypoint);

		while (assembly.MethodsToTranspile.Any()) {
			var method = assembly.MethodsToTranspile.Dequeue();
			ConvertCILToASM(assembly, method);
		}
		
		foreach (var line in AsmFunctions.GetMalloc()) {
			assembly.Add(new ARMLine(-1, header_line++, line));
		}
		if (assembly.IncludeFloat || assembly.IncludeSin) {
			foreach (var line in AsmFunctions.GetFloatFunctions()) {
				assembly.Add(new ARMLine(-1, header_line++, line));
			}
		}
		if (assembly.IncludeSin) {
			foreach (var line in AsmFunctions.GetSinFunctions()) {
				assembly.Add(new ARMLine(-1, header_line++, line));
			}
			foreach (var line in AsmFunctions.GetSinLookupTable()) {
				assembly.Add(new ARMLine(-1, header_line++, line));
			}
		}
		if (assembly.IncludeMGBALog) {
			foreach (var line in AsmFunctions.GetMGBALog()) {
				assembly.Add(new ARMLine(-1, header_line++, line));
			}
		}
		if (assembly.IncludeString || assembly.IncludeMGBALog) {
			foreach (var line in AsmFunctions.GetString()) {
				assembly.Add(new ARMLine(-1, header_line++, line));
			}
		}
		
		assembly.Add(new ARMLine(-1, 1, $"ldr r10, =0x{assembly.HeapStart:X8} @ Heap Start -- WRAM External"));
		return assembly.GetArm7Assembly();
	}

	private ICILMethod DetectEntryPoint() {
		var corHeader = _factory.PEHeaders.CorHeader ?? throw new InvalidDataException("Not a managed assembly.");
		var entryPointToken = corHeader.EntryPointTokenOrRelativeVirtualAddress;
		var entryPointHandle = MetadataTokens.EntityHandle(entryPointToken);
		if (entryPointHandle.Kind != HandleKind.MethodDefinition) throw new InvalidDataException("Entry point is not a managed method.");

		return _factory.GetMethodDefinition(entryPointHandle);
	}

	public void ConvertCILToASM(ARMProgram assembly, ICILMethod method) {
		if (assembly.MethodsTranspiled.Contains(method.FullName)) return;

		var parser = new CILParser();
		var instructions = new ControlFlowGraph(parser.GetInstructions(method.BodyBytes), _factory, method);

		DeclareMethod(assembly, method);

		var locals = method.GetLocalVariableTypes();
		if (_showCil) {
			Console.WriteLine($"{method.FullName}");
			if (locals.Any()) Console.WriteLine($"  locals: {string.Join(", ", method.GetLocalVariableTypes())}");
			instructions.Print();
			Console.WriteLine();
		}

		// r0-r3 = a1-a4 = Argument Registers (Volatile)
		// r4-r8 = v1-v5 = Variable Registers (Saved)
		// r9    = v6    = Reserved
		// r10   = v7    = Heap Pointer (Temporary until we implement malloc/free)
		// r11   = v8/fp = Frame Pointer (Saved)
		// r12   = ip    = Scratch Register (Volatile)
		// r13   = sp    = Stack Pointer (Saved)
		// r14   = lr    = Link Register (Saved)
		// r15   = pc    = Program Counter

		var handlers = new ICILToArmHandler[] {
			new ADD(),
			new AND(),
			new CEQ(),
			new CGT(),
			new CLT(),
			new CONV_I(),
			new CONV_R4(),
			new CONV_U1(),
			new CONV_U2(),
			new DUP(),
			new LDARG_X(method),
			new LDC_I4(),
			new LDC_I4_S(),
			new LDC_I4_X(),
			new LDC_R4(),
			new LDELEM_IX(),
			new LDELEM_REF(),
			new LDIND_U2(),
			new LDLEN(),
			new LDLOCA_S(),
			new LDLOC_X(),
			new LDSTR(_factory),
			new NEWARR(_factory),
			new NOP(),
			new POP(),
			new RET(method),
			new SHL(),
			new STARG_S(method),
			new STELEM_IX(),
			new STELEM_REF(),
			new STLOC_X(),
			new SUB(),
		};

		foreach (var instructionWithMetadata in instructions.Instructions) {
			var instruction = instructionWithMetadata.Instruction;

			var handlerCandidates = handlers.Where(h => h.Handles.Contains(instruction.OpCode));
			if (handlerCandidates.Count() > 1) {
				throw new Exception($"Found multiple handlers for opcode {instruction.OpCode}. Found {string.Join(", ", handlerCandidates)}");
			}
			if (handlerCandidates.Any()) {
				var handler = handlerCandidates.First();
				var result = handler.Handle(instructionWithMetadata);
				assembly.Add(instruction.GetBytes().Length, result.Assembly);
				assembly.IncludeFloat |= result.IncludeFloat;
				assembly.IncludeSin |= result.IncludeSin;
				assembly.IncludeMGBALog |= result.IncludeMGBALog;
				assembly.IncludeString |= result.IncludeString;
				continue;
			}

			var opcode = instruction.OpCode.Name;
			switch (opcode) {
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
				case "stind.i4": {
					assembly.Add(instruction.GetBytes().Length, [
						"pop sp!, { r0, r1 } @ value, addr",
						"str r0, [r1]"
					]);
					break;
				}
				case "ldfld": {
					HandleLoadFieldInstruction(instruction, assembly);
					break;
				}
				case "ldsfld": {
					HandleLoadStaticFieldInstruction(instruction, assembly, method);
					break;
				}
				case "stfld": {
					HandleStoreFieldInstruction(instruction, assembly);
					break;
				}
				case "stsfld": {
					HandleStoreStaticFieldInstruction(instruction, assembly, method);
					break;
				}
				case "call": {
					HandleCallInstruction(instructionWithMetadata, assembly);
					break;
				}
				case "callvirt": {
					HandleCallvirtInstruction(instructionWithMetadata, assembly);
					break;
				}
				case "newobj": {
					HandleNewObjInstruction(instructionWithMetadata, assembly);
					break;
				}
				case "br": {
					var br = (CILBoy.CILParse.Instructions.BR)instruction;
					var label = $"jump_{assembly.JumpCount++}";
					assembly.Add(instruction.GetBytes().Length, [
						$"b {label}"
					]);
					var target = assembly.Offset + br.Target;
					assembly.AddLabel(target, label);
					break;
				}
				case "br.s": {
					var brs = (CILBoy.CILParse.Instructions.BR_S)instruction;
					var label = $"jump_{assembly.JumpCount++}";
					assembly.Add(instruction.GetBytes().Length, [
						$"b {label}"
					]);
					var target = assembly.Offset + brs.Target;
					assembly.AddLabel(target, label);
					break;
				}
				case "brtrue": {
					var brt = (CILBoy.CILParse.Instructions.BRTRUE)instruction;
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
				case "brtrue.s": {
					var brt = (CILBoy.CILParse.Instructions.BRTRUE_S)instruction;
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
				case "brfalse": {
					var brf = (CILBoy.CILParse.Instructions.BRFALSE)instruction;
					var label = $"jump_{assembly.JumpCount++}";
					assembly.Add(instruction.GetBytes().Length, [
						"pop sp!, { r0 }",
						"cmp r0, #0",
						$"beq {label}"
					]);
					var target = assembly.Offset + brf.Target;
					assembly.AddLabel(target, label);
					break;
				}
				case "brfalse.s": {
					var brf = (CILBoy.CILParse.Instructions.BRFALSE_S)instruction;
					var label = $"jump_{assembly.JumpCount++}";
					assembly.Add(instruction.GetBytes().Length, [
						"pop sp!, { r0 }",
						"cmp r0, #0",
						$"beq {label}"
					]);
					var target = assembly.Offset + brf.Target;
					assembly.AddLabel(target, label);
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
				case "not": {
					assembly.Add(instruction.GetBytes().Length, [
						"pop sp!, { r0 }",
						"mvn r0, r0",
						"push sp!, { r0 }"
					]);
					break;
				}
				case "mul": {
					var relevantStack = instructionWithMetadata.StackTypes?.Take(2).ToList() ?? throw new InvalidOperationException($"Stack not deep enough for a mul! {instructionWithMetadata}");
					var stackTypeA = relevantStack[1];
					var stackTypeB = relevantStack[0];
	
					if (stackTypeA.IsInt32Compatible() && stackTypeB.IsInt32Compatible()) {
						assembly.Add(instruction.GetBytes().Length, [
							"pop sp!, { r1, r2 }",
							"mul r0, r1, r2",
							"push sp!, { r0 }"
						]);
					} else if (stackTypeA.IsSingle() && stackTypeB.IsSingle()) {
						assembly.IncludeFloat = true;
						assembly.Add(instruction.GetBytes().Length, [
							"pop sp!, { r0, r1 }",
							"bl gba_float_mul",
							"push sp!, { r0 }"
						]);
					} else {
						throw new NotImplementedException($"CIL 'mul' not supported for types {stackTypeA} * {stackTypeB}. {instructionWithMetadata}");
					}
					break;
				}
				case "div": {
					var relevantStack = instructionWithMetadata.StackTypes?.Take(2).ToList() ?? throw new InvalidOperationException($"Stack not deep enough for an add! {instructionWithMetadata}");
					var stackTypeA = relevantStack[1];
					var stackTypeB = relevantStack[0];

					if (stackTypeA.IsInt32Compatible() && stackTypeB.IsInt32Compatible()) {
						// https://problemkaputt.de/gbatek-bios-arithmetic-functions.htm
						assembly.Add(instruction.GetBytes().Length, [
							"pop sp!, { r0, r1 } @ val2 (denom), val1 (number)",
							"swi 0x070000", // using 7 instead of 6, as the number/denom are swapped
							"push sp!, { r0 }"
						]);
					} else if (stackTypeA.IsSingle() && stackTypeB.IsSingle()) {
						assembly.IncludeFloat = true;
						assembly.Add(instruction.GetBytes().Length, [
							"pop sp!, { r1, r2 } @ val2 (denom), val1 (number)",
							"mov r0, r2",
							"bl gba_float_div",
							"push sp!, { r0 }"
						]);
					} else {
						throw new NotImplementedException($"CIL 'div' not supported for types {stackTypeA} / {stackTypeB}. {instructionWithMetadata}");
					}
					break;
				}
				case "rem": {
					var relevantStack = instructionWithMetadata.StackTypes?.Take(2).ToList() ?? throw new InvalidOperationException($"Stack not deep enough for a rem! {instructionWithMetadata}");
					var stackTypeA = relevantStack[1];
					var stackTypeB = relevantStack[0];

					if (stackTypeA.IsInt32Compatible() && stackTypeB.IsInt32Compatible()) {
						// https://problemkaputt.de/gbatek-bios-arithmetic-functions.htm
						assembly.Add(instruction.GetBytes().Length, [
							"pop sp!, { r0, r1 } @ val2 (denom), val1 (number)",
							"swi 0x070000", // using 7 instead of 6, as the number/denom are swapped
							"push sp!, { r1 }"
						]);
					} else {
						throw new NotImplementedException($"CIL 'rem' not supported for types {stackTypeA} / {stackTypeB}. {instructionWithMetadata}");
					}
					break;
				}
				default: throw new Exception($"Couldn't convert CIL instruction to ARM7 ASM: '{opcode}'.");
			}
		}
		assembly.MethodsTranspiled.Add(method.FullName);
	}

	private void DeclareMethod(ARMProgram assembly, ICILMethod method) {
		var localCount = method.GetLocalVariableTypes().Count();
		assembly.Add(0, [
			$"{GetLabelForMethod(method)}:",
			$"@ {method.ReturnType} {method.FullName}({string.Join(", ", method.GetArgumentTypes())})",
		]);
		if (localCount > 0) {
			var locals =  string.Join(", ", method.GetLocalVariableTypes());
			assembly.Add(0, [
				$"@\tlocals: {locals}",
			]);
		}
		assembly.Add(0, [
			"mov ip, sp",
			$"sub sp, sp, #{localCount * 4}",
			"push sp!, { v1-v5, fp, lr }",
			"mov fp, ip"
		]);
	}

	private void HandleLoadFieldInstruction(CILInstruction instruction, ARMProgram assembly) {
		var ldfld = (CILBoy.CILParse.Instructions.LDFLD)instruction;
		var field = _factory.GetFieldDefinition(ldfld.MetadataToken);
		var classLayout = assembly.GetClassLayout(field.Parent);
		assembly.Add(instruction.GetBytes().Length, [
			$"pop sp!, {{ r0 }} @ {classLayout.FullName}",
			$"ldr r1, [r0, #{classLayout.GetFieldOffset(field)}] @ {field.Name}",
			"push sp!, { r1 }"
		]);
	}
	
	private void HandleLoadStaticFieldInstruction(CILInstruction instruction, ARMProgram assembly, ICILMethod method) {
		var ldsfld = (CILBoy.CILParse.Instructions.LDSFLD)instruction;
		var field = _factory.GetFieldDefinition(ldsfld.MetadataToken);
		
		var staticClass = assembly.GetStaticClassLayout(field.Parent);
		var staticConstructor = staticClass.StaticConstructor;
		assembly.Add(instruction.GetBytes().Length, [
			$"ldr r0, =0x{staticClass.StartAddress:X8} @ static ${staticClass.FullName}",
		]);
		if (staticConstructor != null && method.FullName != staticConstructor.FullName) {
			assembly.MethodsToTranspile.Enqueue(staticConstructor);
			assembly.Add(0, [
				$"ldr r1, [r0]",
				$"cmp r1, #1",
				$"ldrne r1, =1",
				$"strne r1, [r0]",
				$"blne {GetLabelForMethod(staticConstructor)}",
			]);
		}
		assembly.Add(0, [
		 	$"ldr r1, [r0, #{staticClass.GetFieldOffset(field)}] @ {field.Name}",
		 	"push sp!, { r1 }"
		]);
	}

	private void HandleStoreFieldInstruction(CILInstruction instruction, ARMProgram assembly) {
		var stfld = (CILBoy.CILParse.Instructions.STFLD)instruction;
		var field = _factory.GetFieldDefinition(stfld.MetadataToken);

		var classLayout = assembly.GetClassLayout(field.Parent);
		assembly.Add(instruction.GetBytes().Length, [
			"pop sp!, { r0, r1 } @ value, obj",
			$"str r0, [r1, #{classLayout.GetFieldOffset(field)}] @ {field.FullName}"
		]);
	}

	private void HandleStoreStaticFieldInstruction(CILInstruction instruction, ARMProgram assembly, ICILMethod method) {
		var stsfld = (CILBoy.CILParse.Instructions.STSFLD)instruction;
		var field = _factory.GetFieldDefinition(stsfld.MetadataToken);

		var staticClass = assembly.GetStaticClassLayout(field.Parent);
		var staticConstructor = staticClass.StaticConstructor;
		assembly.Add(instruction.GetBytes().Length, [
			$"ldr r0, =0x{staticClass.StartAddress:X8} @ static ${staticClass.FullName}",
		]);
		if (staticConstructor != null && method.FullName != staticConstructor.FullName) {
			assembly.MethodsToTranspile.Enqueue(staticConstructor);
			assembly.Add(0, [
				$"ldr r1, [r0]",
				$"cmp r1, #1",
				$"ldrne r1, =1",
				$"strne r1, [r0]",
				$"blne {GetLabelForMethod(staticConstructor)}",
			]);
		}
		assembly.Add(0, [
		 	"pop sp!, { r1 }",
		 	$"str r1, [r0, #{staticClass.GetFieldOffset(field)}] @ {field.Name}",
		]);
	}

	private void HandleCallInstruction(InstructionMetadata instruction, ARMProgram assembly) {
		var call = (CILBoy.CILParse.Instructions.CALL)instruction.Instruction;
		var method = _factory.GetMethodDefinition(call.MetadataToken);
		HandleCall(instruction, method, assembly, _factory);
	}
	
	private void HandleCallvirtInstruction(InstructionMetadata instruction, ARMProgram assembly) {
		var callvirt = (CILBoy.CILParse.Instructions.CALLVIRT)instruction.Instruction;
		var method = _factory.GetMethodDefinition(callvirt.MetadataToken);
		HandleCall(instruction, method, assembly, _factory);
	}

	private void HandleCall(InstructionMetadata instruction, ICILMethod method, ARMProgram assembly, CILAssemblyFactory factory) {
		var handlers = new List<ICallHandler> {
			new SystemConsoleWriteLine(),
			new SystemConvertToByte(),
			new SystemConvertToInt32(),
			new SystemInt32ToString(),
			new SystemMathFCos(),
			new SystemMathFFloor(),
			new SystemMathFSin(),
			new SystemObjectCtor(),
			new SystemStringConcat(),
		};

		var handler = handlers.FirstOrDefault(h => h.Handles == method.FullName);
		if (handler != null) {
			var code = handler.Handle(instruction, method);
			assembly.Add(instruction.GetBytes().Length, code.Assembly);
			assembly.IncludeFloat |= code.IncludeFloat;
			assembly.IncludeSin |= code.IncludeSin;
			assembly.IncludeMGBALog |= code.IncludeMGBALog;
			assembly.IncludeString |= code.IncludeString;
			return;
		}

		if (method.IsNativeInvoke) {
			if (method.NativeInvokeTarget != "WaitVBlank") throw new Exception("Unrecognized native invoke target");
			assembly.Add(instruction.GetBytes().Length, [
				"swi 0x050000 @ WaitVBlank",
			]);
			return;
		}

		assembly.MethodsToTranspile.Enqueue(method);
		var target = GetLabelForMethod(method);
		assembly.Add(instruction.GetBytes().Length, [
			$"bl {target}"
		]);
	}

	private void HandleNewObjInstruction(InstructionMetadata metadata, ARMProgram assembly) {
		var instruction = metadata.Instruction;
		var newobj = (CILBoy.CILParse.Instructions.NEWOBJ)instruction;
		var handle = MetadataTokens.EntityHandle(newobj.MetadataToken);
		switch (handle.Kind) {
			case HandleKind.MethodDefinition: {
				var method = _factory.GetMethodDefinition((MethodDefinitionHandle)handle);
				if (method.Name != ".ctor") {
					throw new Exception($"Tried to initialize an object with something that isn't a contructor: {method.FullName} -- {metadata}");
				}

				var classLayout = assembly.GetClassLayout(method.Parent);

				var target = GetLabelForMethod(method);
				if (method.ParameterCount == 0) {
					assembly.Add(instruction.GetBytes().Length, [
						$"ldr r0, ={classLayout.Size * 4}",
						"bl gba_malloc",
						"push sp!, { r0 } @ push object ref onto the stack...",
						"push sp!, { r0 } @ push it again for the 'this' param of the constructor",
						$"bl {target}"
					]);
				} else if (method.ParameterCount <= 9) {
					var registers = method.ParameterCount == 1 
						? "r0"
						: $"r0-r{method.ParameterCount - 1}";
					assembly.Add(instruction.GetBytes().Length, [
						$"ldr r0, ={classLayout.Size * 4}",
						"bl gba_malloc",
						"mov ip, r0",
						$"pop sp!, {{ {registers} }}",
						"push sp!, { ip } @ push object ref onto the stack...",
						"push sp!, { ip } @ push it again for the 'this' param of the constructor",
						$"push sp!, {{ {registers} }}",
						$"bl {target}"
					]);
				} else {
					throw new Exception($"Only up to 9 args are supported... {metadata}");
				}

				assembly.MethodsToTranspile.Enqueue(method);
				return;
			}
			default: {
				throw new NotImplementedException($"New Objects for {handle.Kind} constructors not yet implemented. {metadata}");
			}
		}
	}

	private string GetLabelForMethod(ICILMethod method) {
		return $"method_{method.FullName}"
			.Replace(".", "_")
			.Replace("<", "_")
			.Replace(">", "_")
			.Replace("$", "_")
			.Replace("|", "_");
	}
}
