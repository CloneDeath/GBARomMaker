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
		public void RollRightWorks(string line, byte[] expectedData) {
			var compiler = new Compiler();

			var result = compiler.GetOperationForLine(line);
			
			var compiledData = result.SelectMany(r => r.ToBytes());
			compiledData.ShouldBe(expectedData);
		}
	}
}
