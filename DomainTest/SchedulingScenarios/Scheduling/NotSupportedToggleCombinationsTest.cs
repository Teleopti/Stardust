using System.Drawing;
using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.FeatureFlags;
using Teleopti.Ccc.Domain.Forecasting;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.Assignment;
using Teleopti.Ccc.Domain.Scheduling.ShiftCreator;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.TestCommon.FakeRepositories;
using Teleopti.Ccc.TestCommon.IoC;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.SchedulingScenarios.Scheduling
{
	[DomainTest]
	public class NotSupportedToggleCombinationsTest
	{
		public FullScheduling Target;
		public FakePersonRepository PersonRepository;
		public FakeActivityRepository ActivityRepository;
		public FakeSkillRepository SkillRepository;
		public FakeScenarioRepository ScenarioRepository;
		public FakePersonAssignmentRepository AssignmentRepository;
		public FakeSkillDayRepository SkillDayRepository;
		public FakeAgentDayScheduleTagRepository AgentDayScheduleTagRepository;
		public FakeDayOffTemplateRepository DayOffTemplateRepository;

		[Test]
		[Ignore("To be fixed - 44757")]
		[Toggle(Toggles.ResourcePlanner_SchedulingIslands_44757)]
		[ToggleOff(Toggles.ResourcePlanner_MergeTeamblockClassicScheduling_44289)]
		public void ClassicAndIslandsShouldThrow()
		{
			DayOffTemplateRepository.Add(new DayOffTemplate());
			var firstDay = new DateOnly(2016, 05, 30);
			var activity = ActivityRepository.Has("_");
			var skill = SkillRepository.Has("skill", activity);
			var scenario = ScenarioRepository.Has("some name");
			var ruleSet = new WorkShiftRuleSet(new WorkShiftTemplateGenerator(activity, new TimePeriodWithSegment(8, 0, 8, 0, 15), new TimePeriodWithSegment(16, 0, 16, 0, 15), new ShiftCategory("_").WithId()));
			PersonRepository.Has(new ContractWithMaximumTolerance(), new SchedulePeriod(firstDay, SchedulePeriodType.Week, 1), ruleSet, skill);
			SkillDayRepository.Has(skill.CreateSkillDayWithDemand(scenario, firstDay, 1));

			Assert.Throws<InvalidToggleCombinationsException>(() =>
			{
				Target.DoScheduling(DateOnlyPeriod.CreateWithNumberOfWeeks(firstDay, 1));
			}, string.Format(InvalidToggleCombinationsException.ExMessage, Toggles.ResourcePlanner_SchedulingIslands_44757, Toggles.ResourcePlanner_MergeTeamblockClassicScheduling_44289));
		}
	}
}