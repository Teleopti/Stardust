using System;
using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.AgentInfo;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Optimization;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.Legacy.Commands;
using Teleopti.Ccc.Domain.Scheduling.SeatLimitation;
using Teleopti.Ccc.Domain.Scheduling.ShiftCreator;
using Teleopti.Ccc.Domain.Scheduling.TeamBlock.WorkShiftCalculation;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.TestCommon.FakeRepositories;
using Teleopti.Ccc.TestCommon.IoC;
using Teleopti.Ccc.TestCommon.Scheduling;
using Teleopti.Wfm.SchedulingTest.SchedulingScenarios.MaxSeat.TestData;

namespace Teleopti.Wfm.SchedulingTest.SchedulingScenarios.MaxSeat
{
	[DomainTest]
	public class MaxSeatNoRandomnessTest : MaxSeatScenario, IIsolateSystem
	{
		public MaxSeatOptimization Target;
		public FakeTeamRepository TeamRepository;

		[Test]
		public void ShouldNotConsiderOneMissingSeatAsSameAsZeroMissingSeats()
		{
			var site = new Site("_") { MaxSeats = 1 }.WithId();
			var team = new Team { Site = site }.WithDescription(new Description("_"));
			TeamRepository.HasConnectedToCurrentBusinessUnit(team);
			var activity = new Activity("_") { RequiresSeat = true }.WithId();
			var dateOnly = new DateOnly(2016, 10, 25);
			var scenario = new Scenario("_");
			var ruleSet = new WorkShiftRuleSet(new WorkShiftTemplateGenerator(activity, new TimePeriodWithSegment(8, 0, 9, 0, 60), new TimePeriodWithSegment(16, 0, 17, 0, 60), new ShiftCategory("_").WithId()));
			ruleSet.AddLimiter(new ActivityTimeLimiter(activity, TimeSpan.FromHours(8), OperatorLimiter.Equals));
			var agentData16to17_1 = MaxSeatDataFactory.CreateAgentWithAssignment(dateOnly, team, new RuleSetBag(ruleSet), scenario, activity, new TimePeriod(16, 0, 17, 0));
			var agentData16to17_2 = MaxSeatDataFactory.CreateAgentWithAssignment(dateOnly, team, new RuleSetBag(ruleSet), scenario, activity, new TimePeriod(16, 0, 17, 0));
			var agentData8to9 = MaxSeatDataFactory.CreateAgentWithAssignment(dateOnly, team, new RuleSetBag(ruleSet), scenario, activity, new TimePeriod(8, 0, 9, 0));
			var agentData = MaxSeatDataFactory.CreateAgentWithAssignment(dateOnly, team, new RuleSetBag(ruleSet), scenario, activity, new TimePeriod(10, 0, 18, 0));
			var schedules = ScheduleDictionaryCreator.WithData(scenario, dateOnly.ToDateOnlyPeriod(), new[] { agentData.Assignment, agentData16to17_1.Assignment, agentData16to17_2.Assignment, agentData8to9.Assignment });
			var optPreferences = new OptimizationPreferences
			{
				Extra =
				{
					UseTeams = true,
					TeamGroupPage = new GroupPageLight("_", GroupPageType.Hierarchy)
				},
				Advanced =
				{
					UserOptionMaxSeatsFeature = MaxSeatsFeatureOptions.ConsiderMaxSeats
				}
			};

			Target.Optimize(new NoSchedulingProgress(),  dateOnly.ToDateOnlyPeriod(), new[] { agentData.Agent }, schedules, Enumerable.Empty<ISkillDay>(), optPreferences, null);

			schedules[agentData.Agent].ScheduledDay(dateOnly)
				.PersonAssignment()
				.Period.StartDateTime.TimeOfDay.Should()
				.Be.EqualTo(TimeSpan.FromHours(8));
		}

		public void Isolate(IIsolate isolate)
		{
			isolate.UseTestDouble<TakeShiftWithLatestShiftStartIfSameShiftValue>().For<IEqualWorkShiftValueDecider>();
		}
	}
}