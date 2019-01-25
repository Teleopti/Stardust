using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.AgentInfo;
using Teleopti.Ccc.Domain.ApplicationLayer.ResourcePlanner;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.FeatureFlags;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.Assignment;
using Teleopti.Ccc.Domain.Scheduling.ShiftCreator;
using Teleopti.Ccc.Infrastructure.ApplicationLayer;
using Teleopti.Ccc.IocCommon.Toggle;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.TestCommon.FakeRepositories;
using Teleopti.Ccc.TestCommon.IoC;

namespace Teleopti.Wfm.SchedulingTest.SchedulingScenarios.IntradayOptimization
{
	[DomainTest]
	[UseEventPublisher(typeof(SyncInFatClientProcessEventPublisher))]
	[LoggedOnAppDomain]
	public class IntradayOptimizationTeamTest : IntradayOptimizationScenarioTest, IConfigureToggleManager
	{
		public FakeSkillRepository SkillRepository;
		public FakePersonRepository PersonRepository;
		public FakeSkillDayRepository SkillDayRepository;
		public FakeScenarioRepository ScenarioRepository;
		public FakePersonAssignmentRepository PersonAssignmentRepository;
		public IntradayOptimizationFromWeb Target;
		public FakePlanningPeriodRepository PlanningPeriodRepository;
		public FakePreferenceDayRepository PreferenceDayRepository;
		public FakeBusinessUnitRepository BusinessUnitRepository;

		
		[Test]
		public void ShouldMoveBothAgentsToHigherDemandIfTeamSameStartTimeIsUsed()
		{
			
			var activity = ActivityFactory.CreateActivity("_");
			var skill = SkillRepository.Has("_", activity);
			var date = new DateOnly(2015, 10, 12);
			var scenario = ScenarioRepository.Has();
			var schedulePeriod = new SchedulePeriod(date, SchedulePeriodType.Week, 1);
			var shiftCategory = new ShiftCategory("_").WithId();
			var ruleSet = new WorkShiftRuleSet(new WorkShiftTemplateGenerator(activity, new TimePeriodWithSegment(8, 0, 9, 0, 60), new TimePeriodWithSegment(16, 0, 17, 0, 60), shiftCategory));
			ruleSet.AddLimiter(new ContractTimeLimiter(new TimePeriod(8, 8), TimeSpan.Zero));
			var team = new Team().WithDescription(new Description("team1")).WithId();
			BusinessUnitRepository.Has(BusinessUnitFactory.CreateBusinessUnitAndAppend(team).WithId(ServiceLocator_DONTUSE.CurrentBusinessUnit.Current().Id.Value));
			var agent1 = PersonRepository.Has(team,schedulePeriod, ruleSet, skill);
			var agent2 = PersonRepository.Has(team,schedulePeriod, ruleSet, skill);
			var planningPeriod = PlanningPeriodRepository.Has(date, 1);
			SkillDayRepository.Has(new List<ISkillDay>
			{
				skill.CreateSkillDayWithDemandPerHour(scenario, date, TimeSpan.FromMinutes(60), new Tuple<int, TimeSpan>(16, TimeSpan.FromMinutes(65)))
			});
			
			planningPeriod.PlanningGroup.SetTeamSettings(new TeamSettings
			{
				GroupPageType = GroupPageType.Hierarchy,
				TeamSameType = TeamSameType.StartTime
			});
			
			PersonAssignmentRepository.Has(agent1, scenario, activity, shiftCategory, date, new TimePeriod(8, 0, 16, 0));
			PersonAssignmentRepository.Has(agent2, scenario, activity, shiftCategory, date, new TimePeriod(8, 0, 16, 0));

			Target.Execute(planningPeriod.Id.GetValueOrDefault());

			PersonAssignmentRepository.LoadAll().All(x => x.ShiftLayers.First().Period.StartDateTime.Hour == 9).Should().Be.True();
		}

		public void Configure(FakeToggleManager toggleManager)
		{
			toggleManager.Enable(Toggles.ResourcePlanner_TeamSchedulingInPlans_79283);
		}
	}
}