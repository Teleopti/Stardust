using System.Collections.Generic;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Forecasting;
using Teleopti.Ccc.Domain.Forecasting.Angel;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.Forecasting.Angel
{
	public class ForecastingWeightedMeanAbsolutePercentageErrorTest
	{
		[Test]
		public void ShouldCalculateAverageOfAccuracyBetweenForecastingAndHistorical()
		{
			var date1 = new DateOnly(2015, 1, 2);
			var date2 = new DateOnly(2015, 1, 3);
			var workloadDay1 = new WorkloadDay();
			workloadDay1.Create(date1, new Workload(SkillFactory.CreateSkill("Phone")), new List<TimePeriod>());
			workloadDay1.MakeOpen24Hours();
			workloadDay1.TotalStatisticCalculatedTasks = 8d;
			var workloadDay2 = new WorkloadDay();
			workloadDay2.Create(date2, new Workload(SkillFactory.CreateSkill("Phone")), new List<TimePeriod>());
			workloadDay2.MakeOpen24Hours();
			workloadDay2.TotalStatisticCalculatedTasks = 8d;
			var result = new ForecastingWeightedMeanAbsolutePercentageError().Measure(new List<IForecastingTarget>
			{
				new ForecastingTarget(date1, new OpenForWork(true, true))
				{
					Tasks = 10d
				},
				new ForecastingTarget(date2, new OpenForWork(true, true))
				{
					Tasks = 8d
				}
			},
				new TaskOwnerPeriod(DateOnly.MinValue, new List<WorkloadDay>
				{
					workloadDay1,
					workloadDay2
				}, TaskOwnerPeriodType.Other).TaskOwnerDayCollection);

			result.Should().Be.EqualTo(87.5);
		}

		[Test]
		public void ShouldCalculateAccuracyBetweenForecastingAndHistorical()
		{
			var workloadDay1 = new WorkloadDay();
			var date1 = new DateOnly(2015, 1, 2);
			workloadDay1.Create(date1, new Workload(SkillFactory.CreateSkill("Phone")), new List<TimePeriod>());
			workloadDay1.MakeOpen24Hours();
			workloadDay1.TotalStatisticCalculatedTasks = 3d;
			var result = new ForecastingWeightedMeanAbsolutePercentageError().Measure(new List<IForecastingTarget>
			{
				new ForecastingTarget(date1, new OpenForWork(true, true))
				{
					Tasks = 4d
				}
			},
				new TaskOwnerPeriod(DateOnly.MinValue, new List<WorkloadDay>
				{
					workloadDay1
				}, TaskOwnerPeriodType.Other).TaskOwnerDayCollection);

			result.Should().Be.EqualTo(66.667);
		}

		[Test]
		public void ShouldCalculateAccuracyAsZeroIfDifferenceIsTooBig()
		{
			var workloadDay1 = new WorkloadDay();
			var date1 = new DateOnly(2015, 1, 2);
			workloadDay1.Create(date1, new Workload(SkillFactory.CreateSkill("Phone")), new List<TimePeriod>());
			workloadDay1.MakeOpen24Hours();
			workloadDay1.TotalStatisticCalculatedTasks = 1d;
			var result = new ForecastingWeightedMeanAbsolutePercentageError().Measure(new List<IForecastingTarget>
			{
				new ForecastingTarget(date1, new OpenForWork(true, true))
				{
					Tasks = 4d
				}
			},
				new TaskOwnerPeriod(DateOnly.MinValue, new List<WorkloadDay>
				{
					workloadDay1
				}, TaskOwnerPeriodType.Other).TaskOwnerDayCollection);

			result.Should().Be.EqualTo(0);
		}
		
		[Test]
		public void ShouldCalculateWhenBothHistoricalAndForecastingAreZero()
		{
			var date1 = new DateOnly(2015, 1, 2);
			var workloadDay1 = new WorkloadDay();
			workloadDay1.Create(date1, new Workload(SkillFactory.CreateSkill("Phone")), new List<TimePeriod>());
			workloadDay1.MakeOpen24Hours();
			workloadDay1.TotalStatisticCalculatedTasks = 0d;

			var result = new ForecastingWeightedMeanAbsolutePercentageError().Measure(new List<IForecastingTarget>
			{
				new ForecastingTarget(date1, new OpenForWork(true, true))
				{
					Tasks = 0d
				}
			},
				new TaskOwnerPeriod(DateOnly.MinValue, new List<WorkloadDay>
				{
					workloadDay1
				}, TaskOwnerPeriodType.Other).TaskOwnerDayCollection);

			result.Should().Be.EqualTo(100);
		}
	}
}