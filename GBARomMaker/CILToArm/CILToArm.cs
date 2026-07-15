using System;
using GBARomMaker.CILParse;

namespace GBARomMaker.CILToArm;

public class CILToArm {
	public static string[] Translate(CILInstruction[] instructions) {
		var assembly = new ARMProgram {
			new ARMLine(-1, 0, "ldr sp, =0x03000000 @ CIL stack pointer -- WRAM Internal")
		};

		int jump_count = 0;

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
				case "br.s": {
					var brs = (GBARomMaker.CILParse.Instructions.BR_S)instruction;
					var label = $"jump_{jump_count++}";
					assembly.Add(instruction.GetBytes().Length, [
						$"b {label}"
					]);
					var target = assembly.Offset + brs.Target;
					assembly.AddLabel(target, $"{label}:");
					break;
				}
				case "brtrue.s": {
					var brt = (GBARomMaker.CILParse.Instructions.BRTRUE_S)instruction;
					var label = $"jump_{jump_count++}";
					assembly.Add(instruction.GetBytes().Length, [
						"ldmdb sp!, { r0 }",
						"cmp r0, #0",
						$"bne {label}"
					]);
					var target = assembly.Offset + brt.Target;
					assembly.AddLabel(target, $"{label}:");
					break;
				}
				case "add": {
					assembly.Add(instruction.GetBytes().Length, [
						"ldmdb sp!, { r1, r2 }",
						"add r0,r1,r2",
						"stmia sp!, r0"
					]);
					break;
				}
				case "mul": {
					assembly.Add(instruction.GetBytes().Length, [
						"ldmdb sp!, { r1, r2 }",
						"mul r0,r1,r2",
						"stmia sp!, r0"
					]);
					break;
				}
				default: throw new Exception("Couldn't convert instruction to ARM7 ASM: " + opcode);
			}
		}

		return assembly.GetArm7Assembly();
	}
}
