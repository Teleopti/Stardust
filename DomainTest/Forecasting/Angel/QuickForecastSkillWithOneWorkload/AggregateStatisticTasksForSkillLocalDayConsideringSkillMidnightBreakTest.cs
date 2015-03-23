using System;
using System.Collections.Generic;
using System.Linq;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.Forecasting;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.Forecasting.Angel.QuickForecastSkillWithOneWorkload
{
	public class AggregateStatisticTasksForSkillLocalDayConsideringSkillMidnightBreakTest : QuickForecastTest
	{
		private readonly IWorkload _workload;

		public AggregateStatisticTasksForSkillLocalDayConsideringSkillMidnightBreakTest()
		{
			var workload = WorkloadFactory.CreateWorkload(SkillFactory.CreateSkill("Direct sales"));
			var localTimeZone = TimeZoneInfo.FindSystemTimeZoneById("W. Europe Standard Time");
			workload.Skill.TimeZone = localTimeZone;
			workload.Skill.MidnightBreakOffset = TimeSpan.FromHours(2);
			workload.TemplateWeekCollection.ForEach(x => x.Value.MakeOpen24Hours());
			workload.SetId(Guid.NewGuid());
			_workload = workload;
		}

		protected override IWorkload Workload { get { return _workload; }}

		protected override DateOnlyPeriod HistoricalPeriod
		{
			get { return new DateOnlyPeriod(2000, 1, 1, 2000, 1, 2); }
		}

		protected override IEnumerable<StatisticTask> StatisticTasks()
		{
			return new[]
			{
				new StatisticTask {Interval = new DateTime(2000, 1, 1, 23, 15, 0, DateTimeKind.Utc), StatOfferedTasks = 6},
				new StatisticTask {Interval = new DateTime(2000, 1, 2, 11, 15, 0, DateTimeKind.Utc), StatOfferedTasks = 7}
			};
		}

		protected override void Assert(IEnumerable<ISkillDay> modifiedSkillDays)
		{
			var lastDecember = modifiedSkillDays.Single(x => x.CurrentDate == FuturePeriod.StartDate);
			var firstJanuary = modifiedSkillDays.Single(x => x.CurrentDate == FuturePeriod.EndDate);

			Convert.ToInt32(lastDecember.Tasks).Should().Be.EqualTo(6);
			Convert.ToInt32(firstJanuary.Tasks).Should().Be.EqualTo(7);
		} 
	}
}