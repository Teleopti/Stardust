using System;
using System.Collections.Generic;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Forecasting;
using Teleopti.Ccc.Domain.Forecasting.Angel.Accuracy;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.TestCommon.FakeData;


namespace Teleopti.Ccc.DomainTest.Forecasting.Angel.Accuracy
{
	public class ForecastAccuracyCalculatorTest
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
			workloadDay1.TotalStatisticAverageTaskTime = TimeSpan.FromSeconds(10);
			var workloadDay2 = new WorkloadDay();
			workloadDay2.Create(date2, new Workload(SkillFactory.CreateSkill("Phone")), new List<TimePeriod>());
			workloadDay2.MakeOpen24Hours();
			workloadDay2.TotalStatisticCalculatedTasks = 8d;
			workloadDay1.TotalStatisticAverageTaskTime = TimeSpan.FromSeconds(10);
			var forecastTask = new Dictionary<DateOnly, double>();
			forecastTask.Add(date1, 10d);
			forecastTask.Add(date2, 8d);
			var forecastTaskTime = new Dictionary<DateOnly, TimeSpan>();
			forecastTaskTime.Add(date1, TimeSpan.FromSeconds(2));
			forecastTaskTime.Add(date2, TimeSpan.FromSeconds(3));
			var forecastAfterTaskTime = new Dictionary<DateOnly, TimeSpan>();
			forecastAfterTaskTime.Add(date1, TimeSpan.FromSeconds(2));
			forecastAfterTaskTime.Add(date2, TimeSpan.FromSeconds(3));
			var result = new ForecastAccuracyCalculator().Accuracy(forecastTask, forecastTaskTime, forecastAfterTaskTime,
				new TaskOwnerPeriod(DateOnly.MinValue, new List<WorkloadDay>
				{
					workloadDay1,
					workloadDay2
				}, TaskOwnerPeriodType.Other).TaskOwnerDayCollection);

			result.TasksPercentageError.Should().Be.EqualTo(87.5);
		}

		[Test]
		public void ShouldCalculateAccuracyBetweenForecastingAndHistorical()
		{
			var workloadDay1 = new WorkloadDay();
			var date1 = new DateOnly(2015, 1, 2);
			workloadDay1.Create(date1, new Workload(SkillFactory.CreateSkill("Phone")), new List<TimePeriod>());
			workloadDay1.MakeOpen24Hours();
			workloadDay1.TotalStatisticCalculatedTasks = 3d;
			workloadDay1.TotalStatisticAverageTaskTime = TimeSpan.FromSeconds(10);
			var forecastTask = new Dictionary<DateOnly, double>();
			forecastTask.Add(date1, 4d);
			var forecastTaskTime = new Dictionary<DateOnly, TimeSpan>();
			forecastTaskTime.Add(date1, TimeSpan.FromSeconds(2));
			var forecastAfterTaskTime = new Dictionary<DateOnly, TimeSpan>();
			forecastAfterTaskTime.Add(date1, TimeSpan.FromSeconds(2));
			var result = new ForecastAccuracyCalculator().Accuracy(forecastTask, forecastTaskTime, forecastAfterTaskTime,
				new TaskOwnerPeriod(DateOnly.MinValue, new List<WorkloadDay>
				{
					workloadDay1
				}, TaskOwnerPeriodType.Other).TaskOwnerDayCollection);

			result.TasksPercentageError.Should().Be.EqualTo(66.7);
		}

		[Test]
		public void ShouldCalculateAccuracyAsZeroIfDifferenceIsTooBig()
		{
			var workloadDay1 = new WorkloadDay();
			var date1 = new DateOnly(2015, 1, 2);
			workloadDay1.Create(date1, new Workload(SkillFactory.CreateSkill("Phone")), new List<TimePeriod>());
			workloadDay1.MakeOpen24Hours();
			workloadDay1.TotalStatisticCalculatedTasks = 1d;
			workloadDay1.TotalStatisticAverageTaskTime = TimeSpan.FromSeconds(10);
			var forecastTasks = new Dictionary<DateOnly, double>();
			forecastTasks.Add(date1, 4d);
			var forecastTaskTime = new Dictionary<DateOnly, TimeSpan>();
			forecastTaskTime.Add(date1, TimeSpan.FromSeconds(2));
			var forecastAfterTaskTime = new Dictionary<DateOnly, TimeSpan>();
			forecastAfterTaskTime.Add(date1, TimeSpan.FromSeconds(2));
			var result = new ForecastAccuracyCalculator().Accuracy(forecastTasks, forecastTaskTime, forecastAfterTaskTime,
				new TaskOwnerPeriod(DateOnly.MinValue, new List<WorkloadDay>
				{
					workloadDay1
				}, TaskOwnerPeriodType.Other).TaskOwnerDayCollection);

			result.TasksPercentageError.Should().Be.EqualTo(0);
		}
		
		[Test]
		public void ShouldCalculateWhenBothHistoricalAndForecastingAreZero()
		{
			var date1 = new DateOnly(2015, 1, 2);
			var workloadDay1 = new WorkloadDay();
			workloadDay1.Create(date1, new Workload(SkillFactory.CreateSkill("Phone")), new List<TimePeriod>());
			workloadDay1.MakeOpen24Hours();
			workloadDay1.TotalStatisticCalculatedTasks = 0d;
			workloadDay1.TotalStatisticAverageTaskTime = TimeSpan.FromSeconds(10);
			var forecastTask = new Dictionary<DateOnly, double>();
			forecastTask.Add(date1, 0d);
			var forecastTaskTime = new Dictionary<DateOnly, TimeSpan>();
			forecastTaskTime.Add(date1, TimeSpan.FromSeconds(2));
			var forecastAfterTaskTime = new Dictionary<DateOnly, TimeSpan>();
			forecastAfterTaskTime.Add(date1, TimeSpan.FromSeconds(2));
			var result = new ForecastAccuracyCalculator().Accuracy(forecastTask, forecastTaskTime, forecastAfterTaskTime,
				new TaskOwnerPeriod(DateOnly.MinValue, new List<WorkloadDay>
				{
					workloadDay1
				}, TaskOwnerPeriodType.Other).TaskOwnerDayCollection);

			result.TasksPercentageError.Should().Be.EqualTo(100);
		}
	}
}