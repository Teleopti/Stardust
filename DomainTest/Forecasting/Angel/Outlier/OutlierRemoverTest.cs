using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Rhino.Mocks;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Forecasting;
using Teleopti.Ccc.Domain.Forecasting.Angel;
using Teleopti.Ccc.Domain.Forecasting.Angel.Methods;
using Teleopti.Ccc.Domain.Forecasting.Angel.Outlier;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.Forecasting.Angel.Outlier
{
	[TestFixture]
	public class OutlierRemoverTest
	{
		[Test]
		public void ShouldRemoveOutliersAndUseAverageInstead()
		{
			var skill = SkillFactory.CreateSkill("testSkill");
			skill.TimeZone = TimeZoneInfo.FindSystemTimeZoneById("GMT Standard Time");
			var workload = WorkloadFactory.CreateWorkload(skill);

			var historicalDate = new DateOnly(2006, 1, 1);

			var currentDate = DateTime.SpecifyKind(historicalDate.Date, DateTimeKind.Utc);
			var date = currentDate.AddMonths(2).AddDays(3);

			var periodForHelper = generateMockedStatisticsWithOutliers(historicalDate, workload, date);

			var historicalData = new TaskOwnerPeriod(historicalDate, periodForHelper.TaskOwnerDays, TaskOwnerPeriodType.Other);

			var indexVolumes = MockRepository.GenerateMock<IIndexVolumes>();
			var volumes = IndexVolumesFactory.Create();
			indexVolumes.Stub(x => x.Create(historicalData)).Return(volumes);

			var target = new OutlierRemover();

			historicalData.TaskOwnerDayCollection
				.Count.Should().Be.EqualTo(25);
			historicalData.TaskOwnerDayCollection.Single(x => x.CurrentDate == new DateOnly(date))
				.Tasks.Should().Be.EqualTo(1000);

			var result = target.RemoveOutliers(historicalData, new TeleoptiClassic(indexVolumes, target));

			result.TaskOwnerDayCollection
				.Count.Should().Be.EqualTo(25);
			var averageTasks = historicalData.TotalStatisticCalculatedTasks / historicalData.TaskOwnerDayCollection.Count;
			result.TaskOwnerDayCollection.Single(x => x.CurrentDate == new DateOnly(date))
				.Tasks.Should().Be.EqualTo(averageTasks);
		}

		private static TaskOwnerHelper generateMockedStatisticsWithOutliers(DateOnly historicalDate, IWorkload workload,
			DateTime date)
		{
			var periodForHelper = SkillDayFactory.GenerateMockedStatistics(historicalDate, workload);
			IWorkloadDay workloadDay = new WorkloadDay();
			workloadDay.Create(new DateOnly(date), workload, new List<TimePeriod> { new TimePeriod(8, 0, 8, 15) });

			workloadDay.Tasks = 1000;
			workloadDay.AverageAfterTaskTime = TimeSpan.FromSeconds(3);
			workloadDay.AverageTaskTime = TimeSpan.FromSeconds(2);


			workloadDay.TaskPeriodList[0].StatisticTask.StatAbandonedTasks = 0;
			workloadDay.TaskPeriodList[0].StatisticTask.StatAnsweredTasks = 110;
			workloadDay.TaskPeriodList[0].StatisticTask.StatAverageAfterTaskTimeSeconds = 10;
			workloadDay.TaskPeriodList[0].StatisticTask.StatAverageTaskTimeSeconds = 20;
			workloadDay.TaskPeriodList[0].StatisticTask.StatCalculatedTasks = 1000;
			workloadDay.TaskPeriodList[0].StatisticTask.StatOfferedTasks = 110;

			workloadDay.Initialize();

			periodForHelper.TaskOwnerDays.Add(workloadDay);
			return periodForHelper;
		}
	}
}