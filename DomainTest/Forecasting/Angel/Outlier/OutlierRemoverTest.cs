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

			historicalData.TaskOwnerDayCollection.Count.Should().Be.EqualTo(25);
			historicalData.TaskOwnerDayCollection.Single(x => x.CurrentDate == new DateOnly(date))
				.TotalStatisticCalculatedTasks.Should().Be.EqualTo(1000);

			var result = target.RemoveOutliers(historicalData, new TeleoptiClassic(indexVolumes));

			const double outlierFactor = 0.5;
			result.TaskOwnerDayCollection.Count.Should().Be.EqualTo(25);
			var averageTasks = historicalData.TotalStatisticCalculatedTasks / historicalData.TaskOwnerDayCollection.Count;
			result.TaskOwnerDayCollection.Single(x => x.CurrentDate == new DateOnly(date))
				.TotalStatisticCalculatedTasks.Should().Be.EqualTo(averageTasks * (1 + outlierFactor));
		}

		private static TaskOwnerHelper generateMockedStatisticsWithOutliers(DateOnly historicalDate, IWorkload workload,
			DateTime date)
		{
			var periodForHelper = SkillDayFactory.GenerateMockedStatisticsWithValidatedVolumeDays(historicalDate, workload);
			var workloadDay = new WorkloadDay();
			workloadDay.Create(new DateOnly(date), workload, new List<TimePeriod> { new TimePeriod(8, 0, 8, 15) });

			var validatedVolumeDay = new ValidatedVolumeDay(workload, new DateOnly(date))
			{
				ValidatedAverageAfterTaskTime = TimeSpan.FromSeconds(3),
				ValidatedAverageTaskTime = TimeSpan.FromSeconds(2),
				TaskOwner = workloadDay,
				ValidatedTasks = 1000
			};

			periodForHelper.TaskOwnerDays.Add(validatedVolumeDay);
			return periodForHelper;
		}
	}
}