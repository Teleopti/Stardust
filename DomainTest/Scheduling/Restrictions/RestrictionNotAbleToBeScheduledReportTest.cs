using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.AgentInfo;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Forecasting;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.Legacy.Commands;
using Teleopti.Ccc.Domain.Scheduling.Restriction;
using Teleopti.Ccc.Domain.Scheduling.Restrictions;
using Teleopti.Ccc.Domain.Scheduling.ShiftCreator;
using Teleopti.Ccc.DomainTest.SchedulingScenarios;
using Teleopti.Ccc.DomainTest.SchedulingScenarios.Scheduling;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.TestCommon.IoC;
using Teleopti.Ccc.TestCommon.Scheduling;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.Scheduling.Restrictions
{
	[DomainTest]
	[UseIocForFatClient]
	public class RestrictionNotAbleToBeScheduledReportTest : SchedulingScenario
	{
		public RestrictionNotAbleToBeScheduledReport Target;
		public Func<ISchedulerStateHolder> SchedulerStateHolderFrom;

		[Test]
		public void ShouldOnlyReportOnAgentsNotAbleToSchedule()
		{
			var period = new DateOnlyPeriod(2017, 12, 01, 2017, 12, 31);
			var activity = new Activity().WithId();
			var skill = new Skill().For(activity).DefaultResolution(60).WithId().IsOpen();
			var scenario = new Scenario();
			var ruleSet = new WorkShiftRuleSet(new WorkShiftTemplateGenerator(activity,
				new TimePeriodWithSegment(8, 0, 10, 0, 60), new TimePeriodWithSegment(16, 0, 18, 0, 60),
				new ShiftCategory("_").WithId()));
			var agent1 = new Person().WithId()
				.WithPersonPeriod(new RuleSetBag(ruleSet), skill)
				.WithSchedulePeriodOneMonth(period.StartDate);
			agent1.Period(period.StartDate).PersonContract = new PersonContract(new Contract("_"), new PartTimePercentage("_"),
				new ContractScheduleWorkingMondayToFriday());
			var agent2 = new Person().WithId()
				.WithPersonPeriod(new RuleSetBag(ruleSet), skill)
				.WithSchedulePeriodOneMonth(period.StartDate);
			agent2.Period(period.StartDate).PersonContract = new PersonContract(new Contract("_"), new PartTimePercentage("_"),
				new ContractScheduleWorkingMondayToFriday());
			var skillDays = skill.CreateSkillDaysWithDemandOnConsecutiveDays(scenario, period, 1);
			var preferenceDays = new List<IPreferenceDay>();
			foreach (var dateOnly in period.DayCollection())
			{
				if (dateOnly.DayOfWeek == DayOfWeek.Saturday || dateOnly.DayOfWeek == DayOfWeek.Sunday)
					continue;

				preferenceDays.Add(new PreferenceDay(agent1, dateOnly,
					new PreferenceRestriction { WorkTimeLimitation = new WorkTimeLimitation(TimeSpan.FromHours(8), TimeSpan.FromHours(8)) }));

				preferenceDays.Add(new PreferenceDay(agent2, dateOnly,
					new PreferenceRestriction { StartTimeLimitation = new StartTimeLimitation(TimeSpan.FromHours(11), null) }));
			}

			SchedulerStateHolderFrom.Fill(scenario, period, new[] { agent1, agent2 }, preferenceDays, skillDays);

			var result = Target.Create(period.StartDate, new[] {agent1, agent2});

			result.Count().Should().Be.EqualTo(1);
			result.First().Agent.Should().Be.EqualTo(agent2);
			
		}

		[Test]
		public void ShouldReportFirstFoundIssue()
		{
			var period = new DateOnlyPeriod(2017, 12, 01, 2017, 12, 31);
			var activity = new Activity().WithId();
			var skill = new Skill().For(activity).DefaultResolution(60).WithId().IsOpen();
			var scenario = new Scenario();
			var ruleSet = new WorkShiftRuleSet(new WorkShiftTemplateGenerator(activity,
				new TimePeriodWithSegment(8, 0, 10, 0, 60), new TimePeriodWithSegment(16, 0, 18, 0, 60),
				new ShiftCategory("_").WithId()));
			var agent1 = new Person().WithId()
				.WithPersonPeriod(new RuleSetBag(ruleSet), skill)
				.WithSchedulePeriodOneMonth(period.StartDate);
			agent1.Period(period.StartDate).PersonContract = new PersonContract(new Contract("_"), new PartTimePercentage("_"),
				new ContractScheduleWorkingMondayToFriday());
			
			var skillDays = skill.CreateSkillDaysWithDemandOnConsecutiveDays(scenario, period, 1);
			var preferenceDays = new List<IPreferenceDay>();
			foreach (var dateOnly in period.DayCollection())
			{
				preferenceDays.Add(new PreferenceDay(agent1, dateOnly,
					new PreferenceRestriction { StartTimeLimitation = new StartTimeLimitation(TimeSpan.FromHours(8), null) }));
			}

			SchedulerStateHolderFrom.Fill(scenario, period, new[] { agent1 }, preferenceDays, skillDays);

			var result = Target.Create(period.StartDate, new[] { agent1 });

			result.Count().Should().Be.EqualTo(1);
			result.First().Reason.Should().Be.EqualTo(RestrictionNotAbleToBeScheduledReason.TooMuchWorkTimeInPeriod);
			result.First().Period.Should().Be.EqualTo(period);
		}

		public RestrictionNotAbleToBeScheduledReportTest(SeperateWebRequest seperateWebRequest, bool resourcePlannerNoPytteIslands47500, bool resourcePlannerXxl47258) : base(seperateWebRequest, resourcePlannerNoPytteIslands47500, resourcePlannerXxl47258)
		{
		}
	}
}