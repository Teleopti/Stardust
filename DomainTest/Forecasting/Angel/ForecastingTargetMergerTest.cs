using System;
using System.Collections.Generic;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Forecasting;
using Teleopti.Ccc.Domain.Forecasting.Angel;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.TestCommon.FakeData;


namespace Teleopti.Ccc.DomainTest.Forecasting.Angel
{
	public class ForecastingTargetMergerTest
	{
		[Test]
		public void ShouldMergeForecastTargetsWithWorkloadDays()
		{
			var date1 = new DateOnly(2015, 1, 2);
			var date2 = new DateOnly(2015, 1, 3);
			var workloadDay1 = new WorkloadDay();
			workloadDay1.Create(date1, new Workload(SkillFactory.CreateSkill("Phone")), new List<TimePeriod>());
			workloadDay1.MakeOpen24Hours();
			workloadDay1.Tasks = 8d;
			var workloadDay2 = new WorkloadDay();
			workloadDay2.Create(date2, new Workload(SkillFactory.CreateSkill("Phone")), new List<TimePeriod>());
			workloadDay2.MakeOpen24Hours();
			workloadDay2.Tasks = 8d;

			var averageAfterTaskTime1 = new TimeSpan();
			var averageAfterTaskTime2 = new TimeSpan();
			var averageTaskTime1 = new TimeSpan();
			var averageTaskTime2 = new TimeSpan();
			const double tasks1 = 10;
			const double tasks2 = 11;
			new ForecastingTargetMerger().Merge(new List<IForecastingTarget>
			{
				new ForecastingTarget(date1, new OpenForWork(true, true))
				{
					Tasks = tasks1,
					AverageAfterTaskTime = averageAfterTaskTime1,
					AverageTaskTime = averageTaskTime1
				},
				new ForecastingTarget(date2, new OpenForWork(true, true))
				{
					Tasks = tasks2,
					AverageAfterTaskTime = averageAfterTaskTime2,
					AverageTaskTime = averageTaskTime2
				}
			},
				new TaskOwnerPeriod(DateOnly.MinValue, new List<WorkloadDay>
				{
					workloadDay1,
					workloadDay2
				}, TaskOwnerPeriodType.Other).TaskOwnerDayCollection);

			Assert.AreEqual(tasks1,workloadDay1.Tasks,.0001);
			workloadDay1.AverageAfterTaskTime.Should().Be.EqualTo(averageAfterTaskTime1);
			workloadDay1.AverageTaskTime.Should().Be.EqualTo(averageTaskTime1);
			Assert.AreEqual(tasks2, workloadDay2.Tasks, .0001);
			workloadDay2.AverageAfterTaskTime.Should().Be.EqualTo(averageAfterTaskTime2);
			workloadDay2.AverageTaskTime.Should().Be.EqualTo(averageTaskTime2);
		}
	}
}