using GBARomMaker.Compilation;
using Shouldly;

namespace GBARomMaker.Test.Compilation;

[TestFixture]
public abstract class TokenQueue_test {
	[TestFixture]
	public class TokenQueue_ParseRegister_test : Compiler_test {
		[TestCase("r0", 0)]
		[TestCase("r2", 2)]
		[TestCase("a1", 0)]
		[TestCase("a4", 3)]
		[TestCase("v1", 4)]
		[TestCase("v8", 11)]
		[TestCase("fp", 11)]
		[TestCase("ip", 12)]
		[TestCase("sp", 13)]
		[TestCase("lr", 14)]
		[TestCase("pc", 15)]
		public void RegisterIsMappedCorrectly(string registerName, byte expectedRegister) {
			var register = new TokenQueue([registerName], $"{registerName}").ParseRegister(registerName);

			register.ShouldBe(expectedRegister);
		}
	}

}
