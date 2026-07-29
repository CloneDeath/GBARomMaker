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
		var entrypoint = DetectEntryPoint();

		var assembly = new ARMProgram {
			new ARMLine(-1, 0, "ldr sp, =0x03007F00 @ CIL stack pointer -- WRAM Internal"),
			// heap start (added below...)
			new ARMLine(-1, 2, "ldr r0, =gba_irq_handler @ Install IRQ Handler"),
			new ARMLine(-1, 3, "ldr r1, =0x03007FFC"),
			new ARMLine(-1, 4, "str r0, [r1]"),
			new ARMLine(-1, 5, $"b {GetLabelForMethod(entrypoint)}"),

			new ARMLine(-1, 6, $"gba_irq_handler:"),
			//new ARMLine(-1, gba_h++, $"ldr r1, =0x06000006"), // 0x06000004 = 0x7C00
			//new ARMLine(-1, gba_h++, $"ldr r2, =0x7C00"),
			//new ARMLine(-1, gba_h++, $"strh r2, [r1]"),

			//new ARMLine(-1, gba_h++, "ldr r1, =0x4000202 @ REG_IF"), 
			//new ARMLine(-1, gba_h++, "ldrh r2, [r1]"),
			//new ARMLine(-1, gba_h++, "ldr r3, =1"),
			//new ARMLine(-1, gba_h++, "orr r2, r2, r3"),
			//new ARMLine(-1, gba_h++, "strh r2, [r1]"),
			//new ARMLine(-1, gba_h++, $"bx lr"),
			

			// ldr r3, =0x04000000
			// ldr r2, [r3, #0x0200] @ 0x04000200
			// ldr r1, [r3, #0x0208] @ 0x04000208
			// str r3, [r3, #0x0208] @ 0x04000208 = 0x040000000,
			// ldrh r2, [r3, #-8] @ 0x03FFFFF8
			// strh r2, [r3, #-8] @
			new ARMLine(-1, 7, $"ldr r1, =0x03007FF8 @ ICF"),
			new ARMLine(-1, 8, $"ldrh r2, [r1]"),
			new ARMLine(-1, 9, $"orr r2, r2, #1"),
			new ARMLine(-1, 10, $"strh r2, [r1]"),
			new ARMLine(-1, 11, $"ldr r0, =1"),
			new ARMLine(-1, 12, $"ldr r1, =0x04000202 @ IRQ Ack"),
			new ARMLine(-1, 13, $"strh r0, [r1]"),
			new ARMLine(-1, 14, $"bx lr"),
			

			//new ARMLine(-1, gba_h++, "ldr r3, =0x04000000"),
			//new ARMLine(-1, gba_h++, "ldr r2, [r3, #0x200]"),
			//new ARMLine(-1, gba_h++, "ldr r1, [r3, #0x208]"),
			//new ARMLine(-1, gba_h++, "str r3, [r3, #0x208]"),
			//new ARMLine(-1, gba_h++, "mrs r0, SPSR"),
			//new ARMLine(-1, gba_h++, "push { r0, r1, r3, lr }"),
			//new ARMLine(-1, gba_h++, "and r1, r2, r2, lsr #16"),
			//new ARMLine(-1, gba_h++, "ldrh r2, [r3, #-8]"),

// 3000020:       e1822001        orr     r2, r2, r1
// 3000024:       e14320b8        strh    r2, [r3, #-8]
// 3000028:       e59f2084        ldr     r2, [pc, #132]  @ 30000b4 <IntrRet+0x2c>
// 300002c:       e2833c02        add     r3, r3, #512    @ 0x200
//
//03000030 <findIRQ>:
// 3000030:       e5920004        ldr     r0, [r2, #4]
// 3000034:       e3500000        cmp     r0, #0
// 3000038:       0a000003        beq     300004c <no_handler>
// 300003c:       e0100001        ands    r0, r0, r1
// 3000040:       1a000005        bne     300005c <jump_intr>
// 3000044:       e2822008        add     r2, r2, #8
// 3000048:       eafffff8        b       3000030 <findIRQ>
//
//0300004c <no_handler>:
// 300004c:       e1c310b2        strh    r1, [r3, #2]
// 3000050:       e8bd400b        pop     {r0, r1, r3, lr}
// 3000054:       e5831208        str     r1, [r3, #520]  @ 0x208
// 3000058:       e1a0f00e        mov     pc, lr
//
//0300005c <jump_intr>:
// 300005c:       e5922000        ldr     r2, [r2]
// 3000060:       e3520000        cmp     r2, #0
// 3000064:       0afffff8        beq     300004c <no_handler>
//
//03000068 <got_handler>:
// 3000068:       e10f1000        mrs     r1, CPSR
// 300006c:       e3c110df        bic     r1, r1, #223    @ 0xdf
// 3000070:       e381101f        orr     r1, r1, #31
// 3000074:       e129f001        msr     CPSR_fc, r1
// 3000078:       e1c300b2        strh    r0, [r3, #2]
// 300007c:       e52de004        push    {lr}            @ (str lr, [sp, #-4]!)
// 3000080:       e28fe000        add     lr, pc, #0
// 3000084:       e12fff12        bx      r2
//
//03000088 <IntrRet>:
// 3000088:       e49de004        pop     {lr}            @ (ldr lr, [sp], #4)
// 300008c:       e3a03301        mov     r3, #67108864   @ 0x4000000
// 3000090:       e5833208        str     r3, [r3, #520]  @ 0x208
// 3000094:       e10f3000        mrs     r3, CPSR
// 3000098:       e3c330df        bic     r3, r3, #223    @ 0xdf
// 300009c:       e3833092        orr     r3, r3, #146    @ 0x92
// 30000a0:       e129f003        msr     CPSR_fc, r3
// 30000a4:       e8bd400b        pop     {r0, r1, r3, lr}
// 30000a8:       e5831208        str     r1, [r3, #520]  @ 0x208
// 30000ac:       e169f000        msr     SPSR_fc, r0
// 30000b0:       e1a0f00e        mov     pc, lr
// 30000b4:       030000d4        .word   0x030000d4
		};

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

		if (method.IsNativeInvoke) {
			if (method.NativeInvokeTarget != "WaitVBlank") throw new Exception("Unrecognized native invoke target");

			DeclareMethod(assembly, method);
			assembly.Add(0, [
				"swi 0x050000",
				$"sub sp, r7, #{11 * 4}",
				"pop sp!, { r0, r1, r2, r3, r4, r7, r9, r10, r11, r12, lr }",
				"bx lr"
			]);
			assembly.MethodsTranspiled.Add(method.FullName);
			return;
		}

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
				default: throw new Exception($"Couldn't convert CIL instruction to ARM7 ASM: '{opcode}'.");
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
