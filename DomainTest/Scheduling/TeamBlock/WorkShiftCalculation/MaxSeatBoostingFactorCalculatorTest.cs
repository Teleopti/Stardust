using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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
		public void ShouldReturnZeroIfMaxSeatReached()
		{
			Assert.AreEqual(0,_target.GetBoostingFactor(4,4));
		}

		[Test]
		public void ShouldReturnTwoIfMaxSeatTwoSeatsAway()
		{
			Assert.AreEqual(2, _target.GetBoostingFactor(2, 4));
		}

		[Test]
		public void ShouldReturnMaxFactorIfNoSeatIsConsumed()
		{
			Assert.AreEqual(4, _target.GetBoostingFactor(0, 4));
		}
	}
}
