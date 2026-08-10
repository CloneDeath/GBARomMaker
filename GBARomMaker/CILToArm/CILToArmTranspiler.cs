using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection.Metadata;
using System.Reflection.Metadata.Ecma335;
using System.Reflection.PortableExecutable;
using GBARomMaker.CIL;
using GBARomMaker.CILParse;
using GBARomMaker.CILToArm.CallHandlers;
using GBARomMaker.CILToArm.ControlFlow;
using GBARomMaker.CILToArm.Handlers;

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

		var factory = new CILFactory(_peReader, _metadata);

		var parser = new CILParser();
		var instructions = new ControlFlowGraph(parser.GetInstructions(method.BodyBytes), factory, method);

		DeclareMethod(assembly, method);

		var locals = method.GetLocalVariableTypes();
		if (_showCil) {
			Console.WriteLine($"{method.FullName}");
			if (locals.Any()) Console.WriteLine($"  locals: {string.Join(", ", method.GetLocalVariableTypes())}");
			instructions.Print();
			Console.WriteLine();
		}

		// Free Register 1 = r0
		// Free Register 2 = r1
		// Free Register 3 = r2
		// Free Register 4 = r3
		// Free Register 5 = r4
		// Temporary = r6 <- NOT SAVED to stack when going between methods. Used for storing fp and ret
		// Frame Pointer = r7
		// Heap Pointer    = r8 <- Temporary until we implement malloc/free
		// Stack Pointer   = sp/r13
		// Link Register   = lr/r14
		// Program Counter = pc/r15

		var handlers = new ICILToArmHandler[] {
			new CONV_I(),
			new CONV_R4(),
			new CONV_U1(),
			new CONV_U2(),
			new DUP(),
			new LDC_I4_X(),
			new LDELEM_REF(),
			new LDLEN(),
			new LDSTR(factory),
			new NEWARR(factory),
			new NOP(),
			new POP(),
			new SHL(),
			new STELEM_REF(),
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
				continue;
			}

			var opcode = instruction.OpCode.Name;
			switch (opcode) {
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
				case "ldc.r4": {
					var ldc = (GBARomMaker.CILParse.Instructions.LDC_R4)instruction;
					assembly.Add(instruction.GetBytes().Length, [
						$"ldr r0, =0x{ldc.DataRaw:X8}",
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
					assembly.Add(instruction.GetBytes().Length, [
						"pop sp!, { r0 }",
						$"str r0, [r7, #-{(location+1) * 4}] @ local { location }"
					]);;
					break;
				}
				case "stloc.s": {
					var stlocs = (GBARomMaker.CILParse.Instructions.STLOC_S)instruction;
					var location = stlocs.Location;
					assembly.Add(instruction.GetBytes().Length, [
						"pop sp!, { r0 }",
						$"str r0, [r7, #-{(location+1) * 4}] @ local { location }",
					]);;
					break;
				}
				case "ldloc.0":
				case "ldloc.1":
				case "ldloc.2":
				case "ldloc.3": {
					var location = int.Parse(opcode[6].ToString()); // ldloc.X
					assembly.Add(instruction.GetBytes().Length, [
						$"ldr r0, [r7, #-{(location+1) * 4}] @ local { location }",
						"push sp!, { r0 }"
					]);
					break;
				}
				case "ldloc.s": {
					var ldlocs = (GBARomMaker.CILParse.Instructions.LDLOC_S)instruction;
					var location = ldlocs.Location;
					assembly.Add(instruction.GetBytes().Length, [
						$"ldr r0, [r7, #-{(location+1) * 4}] @ local { location }",
						"push sp!, { r0 }"
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
				case "ret": {
					var localCount = method.GetLocalVariableTypes().Count();
					assembly.Add(instruction.GetBytes().Length, [
						method.HasReturnValue ? "pop sp!, { r6 } @ return value" : "nop @ no return value",
						$"sub sp, r7, #{localCount * 4}",
						"ldmdb sp, { r0, r1, r2, r3, r4, r7, lr }",
						$"add sp, sp, #{localCount * 4}"
					]);
					// pop any method parameters
					if (method.ParameterCount > 0 || method.IsInstance) {
						var argsToPop = (method.IsInstance ? 1 : 0) + method.ParameterCount;
						assembly.Add(0, [
							$"add sp, sp, #{argsToPop * 4} @ this: { method.IsInstance }; param count: {method.ParameterCount}"
						]);
					}
					if (method.HasReturnValue) {
						assembly.Add(0, [
							"push sp!, { r6 }",
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
					var br = (GBARomMaker.CILParse.Instructions.BR)instruction;
					var label = $"jump_{assembly.JumpCount++}";
					assembly.Add(instruction.GetBytes().Length, [
						$"b {label}"
					]);
					var target = assembly.Offset + br.Target;
					assembly.AddLabel(target, label);
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
				case "brtrue": {
					var brt = (GBARomMaker.CILParse.Instructions.BRTRUE)instruction;
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
				case "brfalse": {
					var brf = (GBARomMaker.CILParse.Instructions.BRFALSE)instruction;
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
					var brf = (GBARomMaker.CILParse.Instructions.BRFALSE_S)instruction;
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
				case "add": {
					var relevantStack = instructionWithMetadata.StackTypes?.Take(2).ToList() ?? throw new InvalidOperationException($"Stack not deep enough for an add! {instructionWithMetadata}");
					var stackTypeA = relevantStack[0].Code;
					var stackTypeB = relevantStack[1].Code;
	
					var stackTypeAIsInt32Compatible = stackTypeA == SignatureTypeCode.Int32
						|| stackTypeA == SignatureTypeCode.Pointer
						|| stackTypeA == SignatureTypeCode.Byte;

					var stackTypeBIsInt32Compatible = stackTypeB == SignatureTypeCode.Int32
						|| stackTypeB == SignatureTypeCode.Pointer
						|| stackTypeB == SignatureTypeCode.Byte;

					// see Table III.2: Binary Numeric Operations
					if (stackTypeAIsInt32Compatible && stackTypeBIsInt32Compatible) {
						assembly.Add(instruction.GetBytes().Length, [
							$"pop sp!, {{ r1, r2 }} @ <{stackTypeA}, {stackTypeB}>",
							"add r0,r1,r2",
							"push sp!, { r0 }"
						]);
					} else if (stackTypeAIsInt32Compatible && stackTypeB == SignatureTypeCode.Single) {
						assembly.IncludeFloat = true;
						assembly.Add(instruction.GetBytes().Length, [
							$"pop sp!, {{ r0, r1 }} @ <{stackTypeA}, {stackTypeB}>",
							"push sp!, { r1 }",
							"bl gba_int_to_float",
							"pop sp!, { r1 }",
							"bl gba_float_add",
							"push sp!, { r0 }"
						]);
					} else if (stackTypeA == SignatureTypeCode.Single && stackTypeB == SignatureTypeCode.Single) {
						assembly.IncludeFloat = true;
						assembly.Add(instruction.GetBytes().Length, [
							$"pop sp!, {{ r0, r1 }} @ <{stackTypeA}, {stackTypeB}>",
							"bl gba_float_add",
							"push sp!, { r0 }"
						]);
					} else {
						throw new NotImplementedException($"CIL 'add' not supported for types {stackTypeA} + {stackTypeB}. {instructionWithMetadata}");
					}
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
				case "not": {
					assembly.Add(instruction.GetBytes().Length, [
						"pop sp!, { r0 }",
						"mvn r0, r0",
						"push sp!, { r0 }"
					]);
					break;
				}
				case "mul": {
					var relevantStack = instructionWithMetadata.StackTypes?.Take(2).ToList() ?? throw new InvalidOperationException($"Stack not deep enough for an add! {instructionWithMetadata}");
					var stackTypeA = relevantStack[0].Code;
					var stackTypeB = relevantStack[1].Code;
	
					var stackTypeAIsInt32Compatible = stackTypeA == SignatureTypeCode.Int32
						|| stackTypeA == SignatureTypeCode.Pointer
						|| stackTypeA == SignatureTypeCode.Byte;

					var stackTypeBIsInt32Compatible = stackTypeB == SignatureTypeCode.Int32
						|| stackTypeB == SignatureTypeCode.Pointer
						|| stackTypeB == SignatureTypeCode.Byte;

					if (stackTypeAIsInt32Compatible && stackTypeBIsInt32Compatible) {
						assembly.Add(instruction.GetBytes().Length, [
							"pop sp!, { r1, r2 }",
							"mul r0, r1, r2",
							"push sp!, { r0 }"
						]);
					} else if (stackTypeA == SignatureTypeCode.Single && stackTypeB == SignatureTypeCode.Single) {
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
					var stackTypeA = relevantStack[0].Code;
					var stackTypeB = relevantStack[1].Code;
	
					var stackTypeAIsInt32Compatible = stackTypeA == SignatureTypeCode.Int32
						|| stackTypeA == SignatureTypeCode.Pointer
						|| stackTypeA == SignatureTypeCode.Byte;

					var stackTypeBIsInt32Compatible = stackTypeB == SignatureTypeCode.Int32
						|| stackTypeB == SignatureTypeCode.Pointer
						|| stackTypeB == SignatureTypeCode.Byte;

					if (stackTypeAIsInt32Compatible && stackTypeBIsInt32Compatible) {
						// https://problemkaputt.de/gbatek-bios-arithmetic-functions.htm
						assembly.Add(instruction.GetBytes().Length, [
							"pop sp!, { r0, r1 } @ val2 (denom), val1 (number)",
							"swi 0x070000", // using 7 instead of 6, as the number/denom are swapped
							"push sp!, { r0 }"
						]);
					} else if (stackTypeA == SignatureTypeCode.Single && stackTypeB == SignatureTypeCode.Single) {
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
					var relevantStack = instructionWithMetadata.StackTypes?.Take(2).ToList() ?? throw new InvalidOperationException($"Stack not deep enough for an add! {instructionWithMetadata}");
					var stackTypeA = relevantStack[0].Code;
					var stackTypeB = relevantStack[1].Code;
	
					var stackTypeAIsInt32Compatible = stackTypeA == SignatureTypeCode.Int32
						|| stackTypeA == SignatureTypeCode.Pointer
						|| stackTypeA == SignatureTypeCode.Byte;

					var stackTypeBIsInt32Compatible = stackTypeB == SignatureTypeCode.Int32
						|| stackTypeB == SignatureTypeCode.Pointer
						|| stackTypeB == SignatureTypeCode.Byte;

					if (stackTypeAIsInt32Compatible && stackTypeBIsInt32Compatible) {
						// https://problemkaputt.de/gbatek-bios-arithmetic-functions.htm
						assembly.Add(instruction.GetBytes().Length, [
							"pop sp!, { r0, r1 } @ val2 (denom), val1 (number)",
							"swi 0x070000", // using 7 instead of 6, as the number/denom are swapped
							"push sp!, { r1 }"
						]);
					} else {
						throw new NotImplementedException($"CIL 'div' not supported for types {stackTypeA} / {stackTypeB}. {instructionWithMetadata}");
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
			"mov r6, sp",
			$"sub sp, sp, #{localCount * 4}",
			"push sp!, { r0, r1, r2, r3, r4, r7, lr }",
			"mov r7, r6"
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
	
	private void HandleLoadStaticFieldInstruction(CILInstruction instruction, ARMProgram assembly, ICILMethod method) {
		var ldsfld = (GBARomMaker.CILParse.Instructions.LDSFLD)instruction;
		var cilFactory = new CILFactory(_peReader, _metadata);
		var field = cilFactory.GetFieldDefinition(ldsfld.MetadataToken);
		
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
		var stfld = (GBARomMaker.CILParse.Instructions.STFLD)instruction;
		var cilFactory = new CILFactory(_peReader, _metadata);
		var field = cilFactory.GetFieldDefinition(stfld.MetadataToken);

		var classLayout = assembly.GetClassLayout(field.Parent);
		assembly.Add(instruction.GetBytes().Length, [
			"pop sp!, { r0, r1 } @ value, obj",
			$"str r0, [r1, #{classLayout.GetFieldOffset(field)}] @ {field.FullName}"
		]);
	}

	private void HandleStoreStaticFieldInstruction(CILInstruction instruction, ARMProgram assembly, ICILMethod method) {
		var stsfld = (GBARomMaker.CILParse.Instructions.STSFLD)instruction;
		var cilFactory = new CILFactory(_peReader, _metadata);
		var field = cilFactory.GetFieldDefinition(stsfld.MetadataToken);

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
		var call = (GBARomMaker.CILParse.Instructions.CALL)instruction.Instruction;
		var factory = new CILFactory(_peReader, _metadata);
		var method = factory.GetMethodDefinition(call.MetadataToken);
		HandleCall(instruction, method, assembly, factory);
	}
	
	private void HandleCallvirtInstruction(InstructionMetadata instruction, ARMProgram assembly) {
		var callvirt = (GBARomMaker.CILParse.Instructions.CALLVIRT)instruction.Instruction;
		var factory = new CILFactory(_peReader, _metadata);
		var method = factory.GetMethodDefinition(callvirt.MetadataToken);
		HandleCall(instruction, method, assembly, factory);
	}

	private void HandleCall(InstructionMetadata instruction, ICILMethod method, ARMProgram assembly, CILFactory factory) {
		var handlers = new List<ICallHandler> {
			new SystemConsoleWriteLine(factory),
			new SystemMathFCos(),
			new SystemMathFSin(),
			new SystemObjectCtor(factory),
		};

		var handler = handlers.FirstOrDefault(h => h.Handles == method.FullName);
		if (handler != null) {
			var code = handler.Handle(instruction);
			assembly.Add(instruction.GetBytes().Length, code.Assembly);
			assembly.IncludeFloat |= code.IncludeFloat;
			assembly.IncludeSin |= code.IncludeSin;
			assembly.IncludeMGBALog |= code.IncludeMGBALog;
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
		var newobj = (GBARomMaker.CILParse.Instructions.NEWOBJ)instruction;
		var handle = MetadataTokens.EntityHandle(newobj.MetadataToken);
		switch (handle.Kind) {
			case HandleKind.MethodDefinition: {
				var method = _metadata.GetMethodDefinition((MethodDefinitionHandle)handle);
				var methodRef = new CILMethodDefinition(_peReader, _metadata, method);
				if (methodRef.Name != ".ctor") {
					throw new Exception($"Tried to initialize an object with something that isn't a contructor: {methodRef.FullName} -- {metadata}");
				}

				var classLayout = assembly.GetClassLayout(methodRef.Parent);

				var target = GetLabelForMethod(methodRef);
				if (methodRef.ParameterCount == 0) {
					assembly.Add(instruction.GetBytes().Length, [
						"push sp!, { r8 } @ push object ref onto the stack...",
						"push sp!, { r8 } @ push it again for the 'this' param of the constructor",
						$"add r8, r8, #{classLayout.Size}",
						$"bl {target}"
					]);
				} else if (methodRef.ParameterCount <= 5) {
					var registers = methodRef.ParameterCount == 1 
						? "r0"
						: $"r0-r{methodRef.ParameterCount - 1}";
					assembly.Add(instruction.GetBytes().Length, [
						$"pop sp!, {{ {registers} }}",
						"push sp!, { r8 } @ push object ref onto the stack...",
						"push sp!, { r8 } @ push it again for the 'this' param of the constructor",
						$"add r8, r8, #{classLayout.Size}",
						$"push sp!, {{ {registers} }}",
						$"bl {target}"
					]);
				} else {
					throw new Exception($"Only up to 5 args are supported... {metadata}");
				}

				assembly.MethodsToTranspile.Enqueue(methodRef);
				return;
			}
			default: {
				throw new NotImplementedException($"New Objects for {handle.Kind} constructors not yet implemented. {metadata}");
			}
		}
	}

	private string GetLabelForMethod(ICILMethod method) {
		return $"method_{method.FullName}".Replace(".", "_").Replace("<", "_").Replace(">", "_").Replace("$", "_");
	}
}
