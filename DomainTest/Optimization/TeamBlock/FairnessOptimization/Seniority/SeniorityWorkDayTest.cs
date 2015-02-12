using System;
using NUnit.Framework;
using Teleopti.Ccc.Domain.Optimization.TeamBlock.FairnessOptimization.Seniority;
using Teleopti.Ccc.UserTexts;

namespace Teleopti.Ccc.DomainTest.Optimization.TeamBlock.FairnessOptimization.Seniority
{
	public class SeniorityWorkDayTest
	{
		private SeniorityWorkDay _target;

		[Test]
		public void ShouldGetProperties()
		{
			_target = new SeniorityWorkDay(DayOfWeek.Monday, 1);
			Assert.AreEqual(DayOfWeek.Monday, _target.DayOfWeek);
			Assert.AreEqual(1, _target.Rank);
		}

		[Test]
		public void ShouldReturnWeekDayName()
		{
			_target = new SeniorityWorkDay(DayOfWeek.Monday, 1);
			Assert.AreEqual(Resources.Monday, _target.DayOfWeekName);

			_target = new SeniorityWorkDay(DayOfWeek.Tuesday, 1);
			Assert.AreEqual(Resources.Tuesday, _target.DayOfWeekName);

			_target = new SeniorityWorkDay(DayOfWeek.Wednesday, 1);
			Assert.AreEqual(Resources.Wednesday, _target.DayOfWeekName);

			_target = new SeniorityWorkDay(DayOfWeek.Thursday, 1);
			Assert.AreEqual(Resources.Thursday, _target.DayOfWeekName);

			_target = new SeniorityWorkDay(DayOfWeek.Friday, 1);
			Assert.AreEqual(Resources.Friday, _target.DayOfWeekName);

			_target = new SeniorityWorkDay(DayOfWeek.Saturday, 1);
			Assert.AreEqual(Resources.Saturday, _target.DayOfWeekName);

			_target = new SeniorityWorkDay(DayOfWeek.Sunday, 1);
			Assert.AreEqual(Resources.Sunday, _target.DayOfWeekName);
		}
	}
}
