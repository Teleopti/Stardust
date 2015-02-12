using System;
using NUnit.Framework;
using Teleopti.Ccc.Domain.Optimization.TeamBlock.FairnessOptimization.Seniority;

namespace Teleopti.Ccc.DomainTest.Optimization.TeamBlock.FairnessOptimization.Seniority
{
	[TestFixture]
	public class SeniorityWorkDayRanksTest
	{
		private SeniorityWorkDayRanks _target;

		[SetUp]
		public void SetUp()
		{
			_target = new SeniorityWorkDayRanks();
		}

		[Test]
		public void ShouldReturnDefaultRanks()
		{
			var monday = _target.Monday;
			var tuesday = _target.Tuesday;
			var wednesday = _target.Wednesday;
			var thursday = _target.Thursday;
			var friday = _target.Friday;
			var saturday = _target.Saturday;
			var sunday = _target.Sunday;

			Assert.AreEqual(1, monday);
			Assert.AreEqual(2, tuesday);
			Assert.AreEqual(3, wednesday);
			Assert.AreEqual(4, thursday);
			Assert.AreEqual(5, friday);
			Assert.AreEqual(6, saturday);
			Assert.AreEqual(7, sunday);
		}

		[Test]
		public void ShouldReturnRank()
		{
			_target.Monday = 7;
			Assert.AreEqual(7, _target.Monday);
		}

		[Test]
		public void ShouldReturnSortedList()
		{
			_target.Monday = 7;
			_target.Tuesday = 6;
			_target.Wednesday = 5;
			_target.Thursday = 4;
			_target.Friday = 3;
			_target.Saturday = 2;
			_target.Sunday = 1;

			var result = _target.SeniorityWorkDays();
			Assert.AreEqual(7, result.Count);
			Assert.AreEqual(DayOfWeek.Sunday,		result[0].DayOfWeek);
			Assert.AreEqual(DayOfWeek.Saturday,		result[1].DayOfWeek);
			Assert.AreEqual(DayOfWeek.Friday,		result[2].DayOfWeek);
			Assert.AreEqual(DayOfWeek.Thursday,		result[3].DayOfWeek);
			Assert.AreEqual(DayOfWeek.Wednesday,	result[4].DayOfWeek);
			Assert.AreEqual(DayOfWeek.Tuesday,		result[5].DayOfWeek);
			Assert.AreEqual(DayOfWeek.Monday,		result[6].DayOfWeek);
		}
	}
}
