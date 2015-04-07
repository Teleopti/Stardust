using System;
using System.Collections.Generic;
using System.Linq;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Forecasting;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.Forecasting.Angel.QuickForecastSkillWithOneWorkload
{
	public class AggregateStatisticTasksForSkillLocalDayTest : QuickForecastTest
	{
		private readonly IWorkload _workload;

		public AggregateStatisticTasksForSkillLocalDayTest()
		{
			var workload = WorkloadFactory.CreateWorkloadWithFullOpenHours(SkillFactory.CreateSkill("Direct sales"));
			workload.SetId(Guid.NewGuid());
			var localTimeZone = TimeZoneInfo.FindSystemTimeZoneById("W. Europe Standard Time");
			workload.Skill.TimeZone = localTimeZone;
			_workload = workload;
		}

		protected override IWorkload Workload { get { return _workload; }}

		protected override DateOnlyPeriod HistoricalPeriodForForecast
		{
			get { return new DateOnlyPeriod(2001, 1, 1, 2001, 1, 1); }
		}

		protected override IEnumerable<StatisticTask> StatisticTasks()
		{
			return new[]
			{
				new StatisticTask {Interval = new DateTime(2000, 12, 31, 23, 15, 0, DateTimeKind.Utc), StatOfferedTasks = 6},
				new StatisticTask {Interval = new DateTime(2001, 1, 1, 11, 15, 0, DateTimeKind.Utc), StatOfferedTasks = 7}
			};
		}

		protected override void Assert(IEnumerable<ISkillDay> modifiedSkillDays)
		{
			Convert.ToInt32(modifiedSkillDays.Single().Tasks)
				.Should().Be.EqualTo(13);
		}
	}
}