using System.Collections.Generic;
using NUnit.Framework;
using Teleopti.Ccc.Domain.Optimization.IntraIntervalOptimization;

namespace Teleopti.Ccc.DomainTest.Optimization.IntraIntervalOptimization
{
	[TestFixture]
	public class ShiftProjectionCacheIntraIntervalValueCalculatorTest
	{
		private ShiftProjectionCacheIntraIntervalValueCalculator _target;

		[SetUp]
		public void SetUp()
		{
			_target = new ShiftProjectionCacheIntraIntervalValueCalculator();	
		}

		[Ignore, Test]
		public void ShouldCalculate()
		{
			var samplesBefore = new List<int> {1, 1, 1, 10};
			var samplesToAdd = new List<int> {1, 1, 1, 5};
			const double valueBefore = 1 / 10d;
			const double valueAfter = 2 / 15d;
			const double expectedValue = valueBefore - valueAfter;

			var result = _target.Calculate(samplesBefore, samplesToAdd);
			Assert.AreEqual(expectedValue, result);
		}
	}
}
