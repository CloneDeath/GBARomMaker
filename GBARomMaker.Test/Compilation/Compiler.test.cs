using GBARomMaker.Compilation;
using Shouldly;

namespace GBARomMaker.Test.Compilation;

[TestFixture]
public abstract class Compiler_test {
	[TestFixture]
	public class Compiler_GetOperationsForAssembly_test : Compiler_test {
		[TestCase("ldr r0, =0x04000000     @ Display control register", new byte[]{ 0x01, 0x03, 0xA0, 0xE3 })]
		[TestCase("ldr r1, =0x0403         @ Mode 3 + BG2 enabled", new byte[]{ 0x03, 0x10, 0xA0, 0xE3, 0x01, 0x1B, 0x81, 0xE3 })]
		[TestCase("strh r1, [r0]", new byte[]{ 0xB0, 0x10, 0xC0, 0xE1 })]
		[TestCase("mov r1, #0x1F           @ Red", new byte[] { 0x1F, 0x10, 0xA0, 0xE3 })]
		[TestCase("ldr sp, =0x03000000 @ CIL stack pointer -- WRAM Internal", new byte[] { 0x03, 0xD4, 0xA0, 0xE3 })]
		[TestCase("stmia sp!, { r0 }", new byte[] { 0x01, 0x00, 0xAD, 0xE8 })]
		[TestCase("ldmdb sp!, { r0, r1 }", new byte[] { 0x03, 0x00, 0x3D, 0xE9 })]

		// pop is an Alias for ldmia
		[TestCase("ldmia sp!, { r1 }", new byte[] { 0x02, 0x00, 0xBD, 0xE8 })]
		[TestCase("pop sp!, { r1 }",   new byte[] { 0x02, 0x00, 0xBD, 0xE8 })]
		
		// push is an Alias for stmdb
		[TestCase("stmdb sp!, { r1 }", new byte[] { 0x02, 0x00, 0x2D, 0xE9 })]
		[TestCase("push sp!, { r1 }",  new byte[] { 0x02, 0x00, 0x2D, 0xE9 })]
		
		[TestCase("bx lr", new byte[] { 0x1E, 0xFF, 0x2F, 0xE1 })]
		[TestCase("mul r0,r1,r2", new byte[] { 0x91, 0x02, 0x00, 0xE0 })]
		[TestCase("cmp r0, r1", new byte[] { 0x01, 0x00, 0x50, 0xE1 })]
		[TestCase("cmp r0, #10", new byte[] { 0x0A, 0x00, 0x50, 0xE3 })]
		[TestCase("nop", new byte[] { 0x00, 0x00, 0xA0, 0xE1 })]
		[TestCase("movlt r0, #1", new byte[] { 0x01, 0x00, 0xA0, 0xB3 })]
		[TestCase("ldr r0, [r3, #-8]", new byte[] { 0x08, 0x00, 0x13, 0xE5 })]
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
