using NUnit.Framework;
using System;
using System.Linq;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.ShiftCreator;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.TestCommon.FakeRepositories;
using Teleopti.Ccc.TestCommon.IoC;


namespace Teleopti.Ccc.DomainTest.SchedulingScenarios.Scheduling
{
	[DomainTest]
	public class SchedulingPersistTest : SchedulingScenario
	{
		public FullScheduling Target;
		public FakePersonRepository PersonRepository;
		public FakeScenarioRepository ScenarioRepository;
		public FakeSkillDayRepository SkillDayRepository;
		public FakeDayOffTemplateRepository DayOffTemplateRepository;
		public FakePersonAssignmentRepository PersonAssignmentRepository;
		public FakePlanningPeriodRepository PlanningPeriodRepository;
		public FakeSkillRepository SkillRepository;


		[TestCase("UTC")]
		[TestCase("W. Europe Standard Time")]
		[TestCase("Mountain Standard Time")]
		public void ShouldOnlyPersistAssignmentsForChoosenPeriod(string timeZone)
		{
			var firstDay = new DateOnly(2017, 9, 1);
			var period = new DateOnlyPeriod(firstDay, new DateOnly(2017, 9, 30));
			var activity = new Activity("_").WithId();
			var skill = SkillRepository.Has("_", activity);
			var scenario = ScenarioRepository.Has("_");
			var ruleSet = new WorkShiftRuleSet(new WorkShiftTemplateGenerator(activity, new TimePeriodWithSegment(8, 0, 8, 0, 15), new TimePeriodWithSegment(16, 0, 16, 0, 15), new ShiftCategory("_").WithId()));
			var contract = new ContractWithMaximumTolerance();
			DayOffTemplateRepository.Add(new DayOffTemplate());
			PersonRepository.Has(new Person().WithId()
				.InTimeZone(TimeZoneInfo.FindSystemTimeZoneById(timeZone))
				.WithPersonPeriod(ruleSet, contract, skill)
				.WithSchedulePeriodOneWeek(firstDay));
			SkillDayRepository.Has(skill.CreateSkillDayWithDemand(scenario, period.Inflate(10), TimeSpan.FromHours(1)));
			var planningPeriod = PlanningPeriodRepository.Has(period.StartDate,SchedulePeriodType.Month, 1);
			
			Target.DoSchedulingAndDO(planningPeriod.Id.Value);
						
			PersonAssignmentRepository.LoadAll().Count()
				.Should().Be.EqualTo(30);
		}

		public SchedulingPersistTest(ResourcePlannerTestParameters resourcePlannerTestParameters) : base(resourcePlannerTestParameters)
		{
		}
	}
}