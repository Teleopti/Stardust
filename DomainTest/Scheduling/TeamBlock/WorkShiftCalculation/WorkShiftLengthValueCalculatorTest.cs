using NUnit.Framework;
using Teleopti.Ccc.Domain.Scheduling.TeamBlock.WorkShiftCalculation;
using Teleopti.Ccc.Secrets.WorkShiftCalculator;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.Scheduling.TeamBlock.WorkShiftCalculation
{
	[TestFixture]
	public class WorkShiftLengthValueCalculatorTest
	{
		private IWorkShiftLengthValueCalculator _target;

		[SetUp]
		public void Setup()
		{
			_target = new WorkShiftLengthValueCalculator();
		}

		[Test]
		public void	ShouldNotModifyValueIfHintIsFree()
		{
			double result = _target.CalculateShiftValueForPeriod(0.5, 300, WorkShiftLengthHintOption.Free);
			Assert.AreEqual(0.5, result);
		}

		[Test]
		public void ShouldNotModifyValueIfHintIsAverage()
		{
			double result = _target.CalculateShiftValueForPeriod(0.5, 300, WorkShiftLengthHintOption.AverageWorkTime);
			Assert.AreEqual(0.5, result);
		}

		[Test]
		public void ShouldReturnBoostedValueForLongerLengthIfHintIsLong()
		{
			double result1 = _target.CalculateShiftValueForPeriod(0.5, 300, WorkShiftLengthHintOption.Long);
			double result2 = _target.CalculateShiftValueForPeriod(0.5, 360, WorkShiftLengthHintOption.Long);
			Assert.IsTrue(result2 > result1);
		}

		[Test]
		public void ShouldReturnBoostedValueForShorterLengthIfHintIsShort()
		{
			double result1 = _target.CalculateShiftValueForPeriod(0.5, 300, WorkShiftLengthHintOption.Short);
			double result2 = _target.CalculateShiftValueForPeriod(0.5, 360, WorkShiftLengthHintOption.Short);
			Assert.IsTrue(result2 < result1);
		}
	}
}