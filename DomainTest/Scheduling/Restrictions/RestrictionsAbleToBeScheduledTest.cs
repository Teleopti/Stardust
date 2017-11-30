using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.AgentInfo;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Forecasting;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.ResourceCalculation;
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
	public class RestrictionsAbleToBeScheduledTest : SchedulingScenario
	{
		public RestrictionsAbleToBeScheduled Target;
		public DesktopScheduling Target2;
		public Func<ISchedulerStateHolder> SchedulerStateHolderFrom;

		[Test]
		//[Ignore("#47013")]
		public void ShouldAddMissingDaysOffAndReportTrueIfNoRestrictions()
		{
			var date = new DateOnly(2017, 12, 01);
			var period = new DateOnlyPeriod(2017, 12, 01, 2017, 12, 31);
			var activity = new Activity().WithId();
			var skill = new Skill().For(activity).DefaultResolution(60).WithId().IsOpen();
			var scenario = new Scenario();
			var ruleSet = new WorkShiftRuleSet(new WorkShiftTemplateGenerator(activity,
				new TimePeriodWithSegment(10, 0, 10, 0, 60), new TimePeriodWithSegment(17, 0, 19, 0, 60),
				new ShiftCategory("_").WithId()));
			var agent = new Person().WithId()
				.WithPersonPeriod(new RuleSetBag(ruleSet), skill)
				.WithSchedulePeriodOneMonth(date);
			agent.Period(date).PersonContract = new PersonContract(new Contract("_"), new PartTimePercentage("_"), new ContractScheduleWorkingMondayToFriday());
			var skillDays =
				skill.CreateSkillDaysWithDemandOnConsecutiveDays(scenario, period, 1);
			var stateHolder = SchedulerStateHolderFrom.Fill(scenario, period, new[] {agent}, Enumerable.Empty<IPersonAssignment>(), skillDays);

			var result = Target.Execute(agent.VirtualSchedulePeriod(date));
			result.Should().Be.True();

			Target2.Execute(new NoSchedulingCallback(), new SchedulingOptions(), new NoSchedulingProgress(), new []{agent}, period);
			stateHolder.Schedules[agent].CalculatedContractTimeHolderOnPeriod(period).Should().Be
				.EqualTo(TimeSpan.FromHours(168));
		}

		[Test]
		//[Ignore("#47013")]
		public void ShouldReportFalseIfWillWorkRestrictionOnAllDays()
		{
			var date = new DateOnly(2017, 12, 01);
			var period = new DateOnlyPeriod(2017, 12, 01, 2017, 12, 31);
			var activity = new Activity().WithId();
			var skill = new Skill().For(activity).DefaultResolution(60).WithId().IsOpen();
			var scenario = new Scenario();
			var ruleSet = new WorkShiftRuleSet(new WorkShiftTemplateGenerator(activity,
				new TimePeriodWithSegment(10, 0, 10, 0, 60), new TimePeriodWithSegment(17, 0, 19, 0, 60),
				new ShiftCategory("_").WithId()));
			var agent = new Person().WithId()
				.WithPersonPeriod(new RuleSetBag(ruleSet), skill)
				.WithSchedulePeriodOneMonth(date);
			agent.Period(date).PersonContract = new PersonContract(new Contract("_"), new PartTimePercentage("_"), new ContractScheduleWorkingMondayToFriday());
			var skillDays =
				skill.CreateSkillDaysWithDemandOnConsecutiveDays(scenario, period, 1);
			var preferenceDays = new List<IPreferenceDay>();
			foreach (var dateOnly in period.DayCollection())
			{
				preferenceDays.Add(new PreferenceDay(agent, dateOnly,
					new PreferenceRestriction { StartTimeLimitation = new StartTimeLimitation(TimeSpan.FromHours(11), null) }));
			}

			var stateHolder = SchedulerStateHolderFrom.Fill(scenario, period, new[] { agent }, preferenceDays, skillDays);

			var result = Target.Execute(agent.VirtualSchedulePeriod(date));
			result.Should().Be.False();

			Target2.Execute(new NoSchedulingCallback(), new SchedulingOptions(), new NoSchedulingProgress(), new[] { agent }, period);
			stateHolder.Schedules[agent].CalculatedContractTimeHolderOnPeriod(period).Should().Be
				.EqualTo(TimeSpan.Zero);
		}

		public RestrictionsAbleToBeScheduledTest(SeperateWebRequest seperateWebRequest, RemoveClassicShiftCategory resourcePlannerRemoveClassicShiftCat46582, RemoveImplicitResCalcContext removeImplicitResCalcContext46680) : base(seperateWebRequest, resourcePlannerRemoveClassicShiftCat46582, removeImplicitResCalcContext46680)
		{
		}
	}
}