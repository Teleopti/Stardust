using System;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Forecasting;
using Teleopti.Ccc.Infrastructure.Forecasting.Angel;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.Forecasting.Angel.Accuracy
{
	public class LinearRegressionTrendTest
	{
		[Test]
		public void ShouldCalculateTrend()
		{
			var skill = SkillFactory.CreateSkill("testSkill");
			skill.TimeZone = TimeZoneInfo.FindSystemTimeZoneById("GMT Standard Time");
			var workload = WorkloadFactory.CreateWorkload(skill);

			var historicalDate = new DateOnly(2006, 1, 1);
			var periodForHelper = SkillDayFactory.GenerateMockedStatistics(historicalDate, workload);
			var historicalData = new TaskOwnerPeriod(historicalDate, periodForHelper.TaskOwnerDays, TaskOwnerPeriodType.Other);

			var target = new LinearRegressionTrend();
			var result = target.CalculateTrend(historicalData);

			Math.Round(result.Slope, 15).Should().Be.EqualTo(0.051061454194126);
			Math.Round(result.Intercept, 13).Should().Be.EqualTo(-28.4971551072732);
		}
	}
}