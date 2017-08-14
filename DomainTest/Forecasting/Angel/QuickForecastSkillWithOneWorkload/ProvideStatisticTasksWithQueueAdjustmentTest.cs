using System;
using System.Collections.Generic;
using System.Linq;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Forecasting;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.Forecasting.Angel.QuickForecastSkillWithOneWorkload
{
	public class ProvideStatisticTasksWithQueueAdjustmentTest : QuickForecastTest
	{
		protected override IWorkload Workload
		{
			get
			{
				base.Workload.QueueAdjustments = new QueueAdjustment { OfferedTasks = new Percent(0.5) };
				return base.Workload;
			}
		}

		protected override IEnumerable<StatisticTask> StatisticTasks()
		{
			var startDateOnHistoricalPeriod = HistoricalPeriodForForecast.ToDateTimePeriod(SkillTimeZoneInfo()).StartDateTime.AddHours(12);
			return new[]
			{
				new StatisticTask {Interval = startDateOnHistoricalPeriod, StatOfferedTasks = 6}
			};
		}

		protected override void Assert(IEnumerable<ISkillDay> modifiedSkillDays)
		{
			var skillDay = modifiedSkillDays.Single();
			Convert.ToInt32(skillDay.Tasks)
				.Should().Be.EqualTo(3);
		}
	}
}