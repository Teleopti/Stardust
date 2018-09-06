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
using Teleopti.Ccc.Domain.Forecasting.Angel.Trend;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.Forecasting.Angel.Outlier
{
	[TestFixture]
	public class OutlierRemoverTest
	{
		[Test]
		public void ShouldRemoveOutliersAndUse3SigmaInstead()
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
			var volumes = IndexVolumesFactory.CreateDayWeekMonthIndexVolumes();
			indexVolumes.Stub(x => x.Create(historicalData)).Return(volumes);

			historicalData.TaskOwnerDayCollection.Count.Should().Be.EqualTo(25);
			var taskOwner = historicalData.TaskOwnerDayCollection.Single(x => x.CurrentDate == new DateOnly(date));
			taskOwner.TotalStatisticCalculatedTasks.Should().Be.EqualTo(1000);
			((int)taskOwner.TotalStatisticAverageTaskTime.TotalSeconds).Should().Be.EqualTo(1000);

			var forecastMethod = new FakeTeleoptiClassic(indexVolumes);
			var target = new OutlierRemover();
			var result = target.RemoveOutliers(historicalData, forecastMethod, forecastMethod, forecastMethod);

			result.TaskOwnerDayCollection.Count.Should().Be.EqualTo(25);
			var taskOwnerWithoutOutlier = result.TaskOwnerDayCollection.Single(x => x.CurrentDate == new DateOnly(date));
			Math.Round(taskOwnerWithoutOutlier.TotalStatisticCalculatedTasks, 3).Should().Be.EqualTo(677.999);
			Math.Round(taskOwnerWithoutOutlier.TotalStatisticAverageTaskTime.TotalSeconds, 3).Should().Be.EqualTo(677.999);
			Math.Round(taskOwnerWithoutOutlier.TotalStatisticAverageAfterTaskTime.TotalSeconds, 3).Should().Be.EqualTo(677.999);
		}

		[Test]
		public void ShouldRemoveOutliersAndUse3SigmaInsteadWhenConsiderTrend()
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
			var volumes = IndexVolumesFactory.CreateDayWeekMonthIndexVolumes();
			indexVolumes.Stub(x => x.Create(historicalData)).Return(volumes);

			var target = new OutlierRemover();

			historicalData.TaskOwnerDayCollection.Count.Should().Be.EqualTo(25);
			var taskOwner = historicalData.TaskOwnerDayCollection.Single(x => x.CurrentDate == new DateOnly(date));
			taskOwner.TotalStatisticCalculatedTasks.Should().Be.EqualTo(1000);

			var forecastMethod = new FakeTeleoptiClassicWithTrend(indexVolumes, new LinearRegressionTrendCalculator());
			var result = target.RemoveOutliers(historicalData, forecastMethod, forecastMethod, forecastMethod);

			result.TaskOwnerDayCollection.Count.Should().Be.EqualTo(25);
			var taskOwnerWithoutOutlier =
				result.TaskOwnerDayCollection.Single(x => x.CurrentDate == new DateOnly(date));
			Math.Round(taskOwnerWithoutOutlier.TotalStatisticCalculatedTasks, 3).Should().Be.EqualTo(713.136);
		}

		private static TaskOwnerHelper generateMockedStatisticsWithOutliers(DateOnly historicalDate, IWorkload workload, DateTime date)
		{
			var periodForHelper = SkillDayFactory.GenerateMockedStatisticsWithValidatedVolumeDays(historicalDate, workload);
			var workloadDay = new WorkloadDay();
			workloadDay.Create(new DateOnly(date), workload, new List<TimePeriod> { new TimePeriod(8, 0, 8, 15) });

			var validatedVolumeDay = new ValidatedVolumeDay(workload, new DateOnly(date))
			{
				ValidatedAverageAfterTaskTime = TimeSpan.FromSeconds(1000),
				ValidatedAverageTaskTime = TimeSpan.FromSeconds(1000),
				TaskOwner = workloadDay,
				ValidatedTasks = 1000
			};

			periodForHelper.TaskOwnerDays.Add(validatedVolumeDay);
			return periodForHelper;
		}
	}

	public class FakeTeleoptiClassic : TeleoptiClassic
	{
		public FakeTeleoptiClassic(IIndexVolumes indexVolumes)
			: base(indexVolumes)
		{
		}

		public override ForecastMethodType Id
		{
			get { return ForecastMethodType.TeleoptiClassicLongTerm; }
		}
	}

	public class FakeTeleoptiClassicWithTrend : TeleoptiClassicWithTrend
	{
		public FakeTeleoptiClassicWithTrend(IIndexVolumes indexVolumes, ILinearTrendCalculator linearTrendCalculator)
			: base(indexVolumes, linearTrendCalculator)
		{
		}

		public override ForecastMethodType Id
		{
			get { return ForecastMethodType.TeleoptiClassicLongTermWithTrend; }
		}
	}
}