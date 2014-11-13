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

		[Test]
		public void ShouldCalculate()
		{
			var samplesBefore = new List<int> {1, 1, 1, 10};
			var samplesToAdd = new List<int> {1, 1, 1, 5};
			const double valueAfter = 2 / 15d;
			var result = _target.Calculate(samplesBefore, samplesToAdd);
			Assert.AreEqual(valueAfter, result);
		}

		[Test]
		public void ShouldFillSamplesBeforeIfNoValues()
		{
			var samplesBefore = new List<int>();
			var samplesToAdd = new List<int> { 0, 0, 0, 0 };
			const double valueAfter = 1 / 1d;
			var result = _target.Calculate(samplesBefore, samplesToAdd);
			Assert.AreEqual(valueAfter, result);
		}
	}
}
