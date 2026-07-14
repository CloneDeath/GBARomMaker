using GBARomMaker.Compilation;
using Shouldly;

namespace GBARomMaker.Test.Compilation;

[TestFixture]
public abstract class Compiler_test {
	[TestFixture]
	public class Compiler_GetOperationForLine_test : Compiler_test {
		[TestCase("ldr r0, =0x04000000     @ Display control register", new byte[]{ 0x01, 0x03, 0xA0, 0xE3 })]
		[TestCase("ldr r1, =0x0403         @ Mode 3 + BG2 enabled", new byte[]{ 0x03, 0x10, 0xA0, 0xE3, 0x01, 0x1B, 0x81, 0xE3 })]
		[TestCase("strh r1, [r0]", new byte[]{ 0xB0, 0x10, 0xC0, 0xE1 })]
		[TestCase("mov r1, #0x1F           @ Red", new byte[] { 0x1F, 0x10, 0xA0, 0xE3 })]
		[TestCase("ldr sp, =0x03000000 @ CIL stack pointer -- WRAM Internal", new byte[] { 0x03, 0xD4, 0xA0, 0xE3 })]
		[TestCase("stmia sp!, { r0 }", new byte[] { 0x01, 0x00, 0xAD, 0xE8 })]
		[TestCase("ldmdb sp!, { r0, r1 }", new byte[] { 0x03, 0x00, 0x3D, 0xE9 })]
		//[TestCase("bx lr", new byte[] { }]
		public void CompiledAssemblyIsCorrect(string line, byte[] expectedData) {
			var compiler = new Compiler();

			var result = compiler.GetOperationForLine(line);
			
			var compiledData = result.SelectMany(r => r.ToBytes());
			compiledData.ShouldBe(expectedData);
		}
	}

	[TestFixture]
	public class Compiler_ParseRegister_test : Compiler_test {
		[TestCase("r0", 0)]
		[TestCase("r2", 2)]
		[TestCase("sp", 13)]
		[TestCase("lr", 14)]
		[TestCase("pc", 15)]
		public void RegisterIsMappedCorrectly(string registerName, byte expectedRegister) {
			var register = Compiler.ParseRegister(registerName);

			register.ShouldBe(expectedRegister);
		}
	}
}
