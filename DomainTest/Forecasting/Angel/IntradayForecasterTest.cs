using System;
using System.Collections.Generic;
using NUnit.Framework;
using Rhino.Mocks;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Forecasting;
using Teleopti.Ccc.Domain.Forecasting.Angel;
using Teleopti.Ccc.Domain.Forecasting.Angel.LegacyWrappers;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.Forecasting.Angel
{
	public class IntradayForecasterTest
	{
		[Test]
		public void ShouldApplyIntradayTemplate()
		{
			var workload = WorkloadFactory.CreateWorkload(SkillFactory.CreateSkillWithId("skill1"));
			var openHours = new TimePeriod(8, 0, 8, 15);
			workload.TemplateWeekCollection[6].ChangeOpenHours(new[] { openHours });
			var workloadDay = new WorkloadDay();
			workloadDay.Create(new DateOnly(2015, 1, 3), workload, new List<TimePeriod> { openHours });

			var workloadDay2 = new WorkloadDay();
			var dateOnly2 = new DateOnly(2014, 3, 15);
			workloadDay2.Create(dateOnly2, workload, new List<TimePeriod> { openHours });
			workloadDay2.SortedTaskPeriodList[0].StatisticTask.StatCalculatedTasks = 108;

			var templatePeriod = new DateOnlyPeriod(2014, 3, 1, 2014, 5, 31);
			var loadStatistics = MockRepository.GenerateMock<ILoadStatistics>();
			loadStatistics.Stub(x => x.LoadWorkloadDay(workload, templatePeriod)).Return(new IWorkloadDayBase[] {workloadDay2});
			var target = new IntradayForecaster(loadStatistics);

			workloadDay.TemplateReference.DayOfWeek.Should().Be.EqualTo(null);

			target.Apply(workload, templatePeriod, new IWorkloadDayBase[] { workloadDay });

			workloadDay.TemplateReference.DayOfWeek.Should().Be.EqualTo(null);
			workloadDay.SortedTaskPeriodList[0].Task.Tasks.Should().Be.EqualTo(108);
		}
	}
}