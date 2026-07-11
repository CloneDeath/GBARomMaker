using GBARomMaker.Rom.Operations;
using Shouldly;

namespace GBARomMaker.Test.Rom.Operations;

[TestFixture]
public abstract class Move_test {
	[TestFixture]
	public class Move_RollRight_test : Move_test {
		[TestCase(0, 0b1010, 0b1010)]
		[TestCase(6, 0b0001, 0x04000000)]
		public void RollRightWorks(int ror, int nn, int expected) {
			var move = new Move();

			var result = move.RollRight(ror, (uint)nn);

			result.ShouldBe((uint)expected);
		}
	}
}
