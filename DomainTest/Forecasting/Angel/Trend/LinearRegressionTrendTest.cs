using System;
using System.Collections.Generic;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Forecasting;
using Teleopti.Ccc.Domain.Forecasting.Angel.Trend;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.TestCommon.FakeData;


namespace Teleopti.Ccc.DomainTest.Forecasting.Angel.Trend
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

			var target = new LinearRegressionTrendCalculator();
			var result = target.CalculateTrend(historicalData);

			Math.Round(result.Slope, 15).Should().Be.EqualTo(0.051061454194126);
			Math.Round(result.Intercept, 13).Should().Be.EqualTo(-28.4971551072732);
		}

		[Test]
		public void ShouldExcludeZeroWhenCalculateTrend()
		{
			var skill = SkillFactory.CreateSkill("testSkill");
			skill.TimeZone = TimeZoneInfo.FindSystemTimeZoneById("GMT Standard Time");
			var workload = WorkloadFactory.CreateWorkload(skill);

			var historicalDate = new DateOnly(2006, 1, 1);
			var periodForHelper = SkillDayFactory.GenerateMockedStatistics(historicalDate, workload);

			IWorkloadDay workloadDay = new WorkloadDay();
			workloadDay.Create(historicalDate.AddDays(100), workload, new List<TimePeriod> { new TimePeriod(8, 0, 8, 15) });

			workloadDay.Tasks = 0;
			workloadDay.AverageAfterTaskTime = TimeSpan.FromSeconds(3);
			workloadDay.AverageTaskTime = TimeSpan.FromSeconds(2);

			workloadDay.TaskPeriodList[0].StatisticTask.StatAbandonedTasks = 0;
			workloadDay.TaskPeriodList[0].StatisticTask.StatAnsweredTasks = 0;
			workloadDay.TaskPeriodList[0].StatisticTask.StatAverageAfterTaskTimeSeconds = 10;
			workloadDay.TaskPeriodList[0].StatisticTask.StatAverageTaskTimeSeconds = 20;
			workloadDay.TaskPeriodList[0].StatisticTask.StatCalculatedTasks = 0;
			workloadDay.TaskPeriodList[0].StatisticTask.StatOfferedTasks = 0;


			workloadDay.Initialize();

			periodForHelper.TaskOwnerDays.Add(workloadDay);
			var historicalData = new TaskOwnerPeriod(historicalDate, periodForHelper.TaskOwnerDays, TaskOwnerPeriodType.Other);

			var target = new LinearRegressionTrendCalculator();
			var result = target.CalculateTrend(historicalData);

			Math.Round(result.Slope, 15).Should().Be.EqualTo(0.051061454194126);
			Math.Round(result.Intercept, 13).Should().Be.EqualTo(-28.4971551072732);
		}
	}
}