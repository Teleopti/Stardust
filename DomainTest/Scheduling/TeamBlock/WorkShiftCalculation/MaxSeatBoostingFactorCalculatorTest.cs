using NUnit.Framework;
using Teleopti.Ccc.Domain.Scheduling.TeamBlock.WorkShiftCalculation;

namespace Teleopti.Ccc.DomainTest.Scheduling.TeamBlock.WorkShiftCalculation
{
	public class MaxSeatBoostingFactorCalculatorTest
	{
		private MaxSeatBoostingFactorCalculator _target;

		[SetUp]
		public void Setup()
		{
			_target = new MaxSeatBoostingFactorCalculator();
		}

		[Test]
		public void ShouldReturnOneIfMaxSeatEuqalsCalculatedSeat()
		{
			Assert.AreEqual(1,_target.GetBoostingFactor(4,4));
		}

		[Test]
		public void ShouldReturnOneIfMaxSeatIsNotReached()
		{
			Assert.AreEqual(1, _target.GetBoostingFactor(2, 4));
		}

		[Test]
		public void ShouldReturnTwoIfCalculatedSeatIsOverMax()
		{
			Assert.AreEqual(2, _target.GetBoostingFactor(6, 4));
		}
	}
}
