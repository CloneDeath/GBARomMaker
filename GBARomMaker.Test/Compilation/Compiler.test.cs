using GBARomMaker.Compilation;
using Shouldly;

namespace GBARomMaker.Test.Compilation;

[TestFixture]
public abstract class Compiler_test {
	[TestFixture]
	public class Compiler_GetOperationsForAssembly_test : Compiler_test {
		[TestCase("stmia sp!, { r0 }", new byte[] { 0x01, 0x00, 0xAD, 0xE8 })]
		[TestCase("ldmdb sp!, { r0, r1 }", new byte[] { 0x03, 0x00, 0x3D, 0xE9 })]
		[TestCase("ldm sp, { r0, r1, r2, r3, r4, r7, lr }", new byte[] { 0x9F, 0x40, 0x9D, 0xE8 })]
		[TestCase("bx lr", new byte[] { 0x1E, 0xFF, 0x2F, 0xE1 })]
		[TestCase("swi 0x050000", new byte[] { 0x00, 0x00, 0x05, 0xEF })]

		// tests
		[TestCase("cmp r0, r1", new byte[] { 0x01, 0x00, 0x50, 0xE1 })]
		[TestCase("cmp r0, #10", new byte[] { 0x0A, 0x00, 0x50, 0xE3 })]
		[TestCase("teqne r2, r3", new byte[] { 0x03, 0x00, 0x32, 0x11 })]
		[TestCase("tst r0, #0x80000000", new byte[] { 0x02, 0x01, 0x10, 0xE3 })]
		[TestCase("cmn r2, #0x61", new byte[] { 0x61, 0x00, 0x72, 0xE3 })]

		// str variations
		[TestCase("str r0, [r1, #0]", new byte[] { 0x00, 0x00, 0x81, 0xE5 })]
		[TestCase("str r2, [r0, r1]", new byte[] { 0x01, 0x20, 0x80, 0xE7 })]
		[TestCase("strne r1, [r0]", new byte[] { 0x00, 0x10, 0x80, 0x15 })]
		[TestCase("strh r1, [r0]", new byte[]{ 0xB0, 0x10, 0xC0, 0xE1 })]
		[TestCase("strb r0, [r1]", new byte[] { 0x00, 0x00, 0xC1, 0xE5 })]
		
		// ldr variations
		[TestCase("ldr r0, =0x04000000     @ Display control register", new byte[]{ 0x01, 0x03, 0xA0, 0xE3 })]
		[TestCase("ldr r1, =0x0403         @ Mode 3 + BG2 enabled", new byte[]{ 0x03, 0x10, 0xA0, 0xE3, 0x01, 0x1B, 0x81, 0xE3 })]
		[TestCase("ldr sp, =0x03000000 @ CIL stack pointer -- WRAM Internal", new byte[] { 0x03, 0xD4, 0xA0, 0xE3 })]
		[TestCase("ldr r0, [sp]", new byte[] { 0x00, 0x00, 0x9D, 0xE5 })]
		[TestCase("ldr r0, [r3, #-8]", new byte[] { 0x08, 0x00, 0x13, 0xE5 })]
		[TestCase("ldrne r1, =1", new byte[] { 0x01, 0x10, 0xA0, 0x13 })]
		[TestCase("ldrh r2, [r3]", new byte[] { 0xB0, 0x20, 0xD3, 0xE1 })]
		[TestCase("ldr r2, [r0, r1, lsl #2]", new byte[] { 0x01, 0x21, 0x90, 0xE7 })]

		// nop
		[TestCase("nop", new byte[] { 0x00, 0x00, 0xA0, 0xE1 })]

		// pop is an Alias for ldmia
		[TestCase("ldmia sp!, { r1 }", new byte[] { 0x02, 0x00, 0xBD, 0xE8 })]
		[TestCase("pop sp!, { r1 }",   new byte[] { 0x02, 0x00, 0xBD, 0xE8 })]
		
		// push is an Alias for stmdb
		[TestCase("stmdb sp!, { r1 }", new byte[] { 0x02, 0x00, 0x2D, 0xE9 })]
		[TestCase("push sp!, { r1 }",  new byte[] { 0x02, 0x00, 0x2D, 0xE9 })]

		// ALU
		[TestCase("orr r0, r1, r2", new byte[] { 0x02, 0x00, 0x81, 0xE1 })]
		[TestCase("mov r1, #0x1F @ Red", new byte[] { 0x1F, 0x10, 0xA0, 0xE3 })]
		[TestCase("movs r4, r0", new byte[] { 0x00, 0x40, 0xB0, 0xE1 })]
		[TestCase("movlt r0, #1", new byte[] { 0x01, 0x00, 0xA0, 0xB3 })]
		[TestCase("mvn r2, r2", new byte[] { 0x02, 0x20, 0xE0, 0xE1 })]
		[TestCase("mvnsne ip, r2, asr #24", new byte[] { 0x42, 0xCC, 0xF0, 0x11 })]
		[TestCase("rsbs r3, r2, r3, lsr #24", new byte[] { 0x23, 0x3C, 0x72, 0xE0 })]
		[TestCase("andsne r3, r4, r1, lsr #23", new byte[] { 0xA1, 0x3B, 0x14, 0x10 })]

		// MUL
		[TestCase("mul r0,r1,r2", new byte[] { 0x91, 0x02, 0x00, 0xE0 })]
		[TestCase("umull r3, r1, r0, r1", new byte[] { 0x90, 0x31, 0x81, 0xE0 })]
		
		// lsl/lsr is a psudocommand for mov with logical shift left/right
		[TestCase("mov r2, r0, lsl r1", new byte[] { 0x10, 0x21, 0xA0, 0xE1 })]
		[TestCase("lsl r2, r0, r1", new byte[] { 0x10, 0x21, 0xA0, 0xE1 })]
		[TestCase("mov r2, r2, lsr #24", new byte[] { 0x22, 0x2C, 0xA0, 0xE1 })]
		[TestCase("lsr r2, r2, #24", new byte[] { 0x22, 0x2C, 0xA0, 0xE1 })]
		[TestCase("lsls r2, r0, #1", new byte[] { 0x80, 0x20, 0xB0, 0xE1 })]
		[TestCase("lslsne r3, r1, #1", new byte[] { 0x81, 0x30, 0xB0, 0x11 })]

		// rrx is a special mov alias, is it only ever has a value of 1 (which maps to ror#0)
		[TestCase("mov r1, r1, rrx #1", new byte[] { 0x61, 0x10, 0xA0, 0xE1 })]
		[TestCase("rrx r1, r1", new byte[] { 0x61, 0x10, 0xA0, 0xE1 })]
		[TestCase("rrxs r0, r1", new byte[] { 0x61, 0x00, 0xB0, 0xE1 })]
		public void CompiledAssemblyIsCorrect(string line, byte[] expectedData) {
			var compiler = new Compiler();

			var result = compiler.GetOperationsForAssembly([line]);
			
			var compiledData = result.ToBytes();
			compiledData.ShouldBe(expectedData);
		}

		[Test]
		public void JumpsBackAreCalculatedCorrectly() {
			var compiler = new Compiler();
			var assembly = new string[]{
				"nop",
				"target:",
				"nop",
				"nop",
				"b target"
			};

			var result = compiler.GetOperationsForAssembly(assembly);

			result.LabelsAreMissing.ShouldBe(false);
			var compiledData = result.ToBytes();
			compiledData.ShouldBe(new byte[] {
				0x00, 0x00, 0xA0, 0xE1, // nop
				0x00, 0x00, 0xA0, 0xE1, // target: nop
				0x00, 0x00, 0xA0, 0xE1, // nop
				0xFC, 0xFF, 0xFF, 0xEA  // b target
			});
		}
		
		[Test]
		public void JumpsForwardAreCalculatedCorrectly() {
			var compiler = new Compiler();
			var assembly = new string[]{
				"b target",
				"nop",
				"nop",
				"target:",
				"nop",
			};

			var result = compiler.GetOperationsForAssembly(assembly);

			result.LabelsAreMissing.ShouldBe(false);
			var compiledData = result.ToBytes();
			compiledData.ShouldBe(new byte[] {
				0x01, 0x00, 0x00, 0xEA, // b target
				0x00, 0x00, 0xA0, 0xE1, // nop
				0x00, 0x00, 0xA0, 0xE1, // nop
				0x00, 0x00, 0xA0, 0xE1  // target: nop
			});
		}
	}
}
