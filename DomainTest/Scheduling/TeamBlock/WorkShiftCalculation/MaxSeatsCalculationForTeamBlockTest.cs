using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using Teleopti.Ccc.Domain.Scheduling.TeamBlock.WorkShiftCalculation;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.Scheduling.TeamBlock.WorkShiftCalculation
{
	[TestFixture]
	public class MaxSeatsCalculationForTeamBlockTest
	{
		private MaxSeatsCalculationForTeamBlock _target;

		[SetUp]
		public void SetUp()
		{
			_target = new MaxSeatsCalculationForTeamBlock();
		}

		[Test]
		public void ShouldReturnOriginalPeriodValueIfNotConsiderMaxSeats()
		{
			Assert.AreEqual(12.45,
				_target.PeriodValue(12.45, MaxSeatsFeatureOptions.DoNotConsiderMaxSeats, false, true,25));
		}

		[Test]
		public void ShouldReturnOriginalPeriodValueIfNotConsiderMaxSeatsWhenMaxSeatsReached()
		{
			Assert.AreEqual(12.45,
				_target.PeriodValue(12.45, MaxSeatsFeatureOptions.DoNotConsiderMaxSeats, true, true,25));
		}

		[Test]
		public void ShouldReturnOriginalPeriodValueIfConsiderMaxSeats()
		{
			Assert.AreEqual(12.45,
				_target.PeriodValue(12.45, MaxSeatsFeatureOptions.ConsiderMaxSeats, false, true,1));
		}

		[Test]
		public void ShouldReturnAPunishedPeriodValueIfConsiderMaxSeatsWhenMaxSeatsReached()
		{
			Assert.AreEqual(12.45 * -10000*10,
				_target.PeriodValue(12.45, MaxSeatsFeatureOptions.ConsiderMaxSeats, true, true,10));
		}

		[Test]
		public void ShouldReturnNullIfConsiderMaxSeatsAndDontBreak()
		{
			Assert.IsNull(
				_target.PeriodValue(12.45, MaxSeatsFeatureOptions.ConsiderMaxSeatsAndDoNotBreak, true, true,11));
		}

		[Test]
		public void ShouldReturnOriginalPeriodValueIfConsiderMaxSeatsAndDontBreak()
		{
			Assert.AreEqual(12.45,
				_target.PeriodValue(12.45, MaxSeatsFeatureOptions.ConsiderMaxSeatsAndDoNotBreak, false, true,1));
		}

		[Test]
		public void ReturnSameValueIfRequireSeatIsFalse()
		{
			Assert.AreEqual(12.45,
				_target.PeriodValue(12.45, MaxSeatsFeatureOptions.ConsiderMaxSeatsAndDoNotBreak, false, false,1));
		}
	}
}
