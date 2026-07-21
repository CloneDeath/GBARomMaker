using Shouldly;

namespace GBARomMaker.Test.Compilation;

[TestFixture]
public abstract class TokenQueue_test {
	[TestFixture]
	public class TokenQueue_ParseRegister_test : Compiler_test {
		[TestCase("r0", 0)]
		[TestCase("r2", 2)]
		[TestCase("sp", 13)]
		[TestCase("lr", 14)]
		[TestCase("pc", 15)]
		public void RegisterIsMappedCorrectly(string registerName, byte expectedRegister) {
			var register = new TokenQueue([], "").ParseRegister(registerName);

			register.ShouldBe(expectedRegister);
		}
	}

}
