using System;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Forecasting;
using Teleopti.Ccc.Domain.Forecasting.Angel.Methods;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.Forecasting.Angel.Methods
{
	[TestFixture]
	public class SimpleAhtAndAcwCalculatorTest
	{
		[Test]
		public void ShouldGetRecent3MonthsAverageForAhtAndAcw()
		{
			var skill = SkillFactory.CreateSkill("testSkill");
			skill.TimeZone = TimeZoneInfo.FindSystemTimeZoneById("GMT Standard Time");
			var workload = WorkloadFactory.CreateWorkload(skill);

			var historicalDate = new DateOnly(2006, 1, 1);
			var periodForHelper = SkillDayFactory.GenerateMockedStatistics(historicalDate, workload);
			var historicalData = new TaskOwnerPeriod(historicalDate, periodForHelper.TaskOwnerDays, TaskOwnerPeriodType.Other);

			var target = new SimpleAhtAndAcwCalculator();
			var ahtAndAcw = target.Recent3MonthsAverage(historicalData);
			ahtAndAcw.Acw.Should().Be.EqualTo(TimeSpan.FromSeconds(10));
			ahtAndAcw.Aht.Should().Be.EqualTo(TimeSpan.FromSeconds(20));
		}
	}
}