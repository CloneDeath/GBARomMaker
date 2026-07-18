using GBARomMaker.ARM.ALU;
using Shouldly;

namespace GBARomMaker.Test.ARM.ALU;

[TestFixture]
public abstract class Immediate_test {
	[TestFixture]
	public class Immediate_RollRight_test : Immediate_test {
		[TestCase(0, 0b1010, 0b1010)]
		[TestCase(6, 0b0001, 0x04000000)]
		[TestCase(8, 0b0011, 0x03000000)]
		public void RollRightWorks(int ror, int nn, int expected) {
			var move = new Immediate();

			var result = move.RollRight(ror, (uint)nn);

			result.ShouldBe((uint)expected);
		}
	}
}
