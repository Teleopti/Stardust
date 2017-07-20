using System;
using System.Collections.Generic;
using NUnit.Framework;
using Teleopti.Ccc.Domain.AgentInfo;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Optimization;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.Legacy.Commands;
using Teleopti.Ccc.Domain.Scheduling.ScheduleTagging;
using Teleopti.Ccc.Domain.Scheduling.ShiftCreator;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeRepositories;
using Teleopti.Ccc.TestCommon.IoC;
using Teleopti.Interfaces.Domain;


namespace Teleopti.Ccc.DomainTest.SchedulingScenarios.TimeBetweenDaysOptimization
{
	[DomainTest]
	[UseIocForFatClient]
	public class TimeBetweenDaysOptimizationTeamBlockDesktopTest
	{
		public Func<ISchedulerStateHolder> SchedulerStateHolder;
		public OptimizationExecuter Target;
		public FakeRuleSetBagRepository RuleSetBagRepository;

		[Test]
		public void ShouldNotCrashOnAgentWithLeavingDate()
		{
			var date = new DateOnly(2015, 10, 12);
			var period = DateOnlyPeriod.CreateWithNumberOfWeeks(date, 1);
			var ruleSet = new WorkShiftRuleSet(new WorkShiftTemplateGenerator(new Activity("_"), new TimePeriodWithSegment(8, 0, 8, 0, 15), new TimePeriodWithSegment(16, 0, 16, 0, 15), new ShiftCategory("_").WithId()));
			var ruleSetBag = new RuleSetBag(ruleSet){Description = new Description("_")};
			RuleSetBagRepository.Has(ruleSetBag);
			var team = new Team {Site = new Site("_")};
			var contract = new ContractWithMaximumTolerance();
			var agent1 = new Person().WithId().InTimeZone(TimeZoneInfo.Utc).WithPersonPeriod(ruleSetBag, contract, team).WithSchedulePeriodOneWeek(date);
			var agent2 = new Person().WithId().InTimeZone(TimeZoneInfo.Utc).WithPersonPeriod(ruleSetBag, contract, team).WithSchedulePeriodOneWeek(date);
			var stateHolder = SchedulerStateHolder.Fill(new Scenario("_"), period, new []{agent1, agent2}, new List<IScheduleData>(), new List<ISkillDay>());
			agent1.TerminatePerson(date.AddDays(1), new PersonAccountUpdaterDummy());
			var optPreferences = new OptimizationPreferences
			{
				General = {ScheduleTag = new ScheduleTag(), OptimizationStepTimeBetweenDays = true},
				Extra = {UseTeams = true, TeamGroupPage = new GroupPageLight("_", GroupPageType.RuleSetBag)}
			};

			Assert.DoesNotThrow(() =>
			{
				Target.Execute(new NoSchedulingProgress(), stateHolder, new[] { agent1, agent2 }, period, optPreferences, null);
			});
		}
	}
}
