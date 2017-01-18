using System;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.AgentInfo;
using Teleopti.Ccc.Domain.ApplicationLayer.Events;
using Teleopti.Ccc.Domain.Common.Time;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.Restriction;
using Teleopti.Ccc.Domain.Scheduling.ShiftCreator;
using Teleopti.Ccc.Domain.Scheduling.TimeLayer;
using Teleopti.Ccc.Domain.Staffing;
using Teleopti.Ccc.IocCommon;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.TestCommon.FakeRepositories;
using Teleopti.Ccc.TestCommon.IoC;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.DomainTest.Staffing
{
	[DomainTest]
	[TestFixture]
	public class AddOverTimeHandlerTest : ISetup
	{
		public AddOverTimeHandler Target;
		public MutableNow Now;
		public FakePersonRepository PersonRepository;
		public FakeScenarioRepository ScenarioRepository;
		public FakeActivityRepository ActivityRepository;
		public FakeSkillRepository SkillRepository;
		public FakePersonAssignmentRepository PersonAssignmentRepository;
		public FakeSkillDayRepository SkillDayRepository;
		public FakeMultiplicatorDefinitionSetRepository MultiplicatorDefinitionSetRep;
		public FakeOvertimeAvailabilityRepository OvertimeAvailabilityRepository;
		public FakeRuleSetBagRepository RuleSetBagRepository;
		public IScheduleStorage ScheduleStorage;
		public Func<ISchedulingResultStateHolder> SchedulingResultStateHolder;

		public void Setup(ISystem system, IIocConfiguration configuration)
		{
			system.UseTestDouble<AddOverTimeHandler>().For<AddOverTimeHandler>();
			system.UseTestDouble<FakeScheduleDifferenceSaver>().For<IScheduleDifferenceSaver>();
		}

		[Test] 
		public void ShouldAddOverTimeForPeriodAndSkill()
		{
			Now.Is("2017-1-11 13:00");
			var multi = new MultiplicatorDefinitionSet("kulti", MultiplicatorType.Overtime).WithId();
			MultiplicatorDefinitionSetRep.Add(multi);
			var period = new DateTimePeriod(2017, 1, 11, 7, 2017, 01, 11, 15);

			var scenario = ScenarioRepository.Has("scenario");
			var activity = ActivityRepository.Has("activity");
			var skill = SkillRepository.Has("skillA", activity).WithId();
			skill.DefaultResolution = 60;
			var agent = PersonRepository.Has(skill);

			var team = new Team();
			var contract = ContractFactory.CreateContract("con");
			contract.AddMultiplicatorDefinitionSetCollection(multi);
			var personContract = PersonContractFactory.CreatePersonContract(contract);
			var personPeriod = new PersonPeriod(new DateOnly(2000, 1, 1), personContract, team);
			personPeriod.AddPersonSkill(new PersonSkill(skill, new Percent(1)));
			agent.AddPersonPeriod(personPeriod);

			OvertimeAvailabilityRepository.Add(new OvertimeAvailability(agent, new DateOnly(2017,1,11),TimeSpan.FromHours(12), TimeSpan.FromHours(20)));

			PersonAssignmentRepository.Has(PersonAssignmentFactory.CreateAssignmentWithMainShift(agent, scenario, activity, period, new ShiftCategory("category")));
			SkillDayRepository.Has(skill.CreateSkillDaysWithDemandOnConsecutiveDays(scenario, new DateOnly(period.StartDateTime), 5));

			
			Target.Handle(new AddOverTimeEvent
			{
				OvertimeDurationMin = TimeSpan.FromHours(1),
				OvertimeDurationMax = TimeSpan.FromHours(3),
				OvertimeType = multi.Id.Value,
				Skills = new[] {skill.Id.GetValueOrDefault()}
			});
			var days = SchedulingResultStateHolder().Schedules[agent];
			var overtimes = days.ScheduledDay(new DateOnly(2017, 1, 11)).PersonAssignment().OvertimeActivities();
			overtimes.Should().Not.Be.Empty();

		}

		[Test]
		public void ShouldAddOverTimeOnEmptyDayIfAvailable()
		{
			Now.Is("2017-1-11 13:00");
			var multi = new MultiplicatorDefinitionSet("kulti", MultiplicatorType.Overtime).WithId();
			MultiplicatorDefinitionSetRep.Add(multi);
			var period = new DateTimePeriod(2017, 1, 11, 7, 2017, 01, 11, 15);

			var category = ShiftCategoryFactory.CreateShiftCategory("katten");
			var scenario = ScenarioRepository.Has("scenario");
			var activity = ActivityRepository.Has("activity");
			var skill = SkillRepository.Has("skillA", activity).WithId();
			skill.DefaultResolution = 60;
			var agent = PersonRepository.Has(skill);

			var team = new Team();
			var contract = ContractFactory.CreateContract("con");
			contract.AddMultiplicatorDefinitionSetCollection(multi);
			var personContract = PersonContractFactory.CreatePersonContract(contract);
			var personPeriod = new PersonPeriod(new DateOnly(2000, 1, 1), personContract, team);
			personPeriod.AddPersonSkill(new PersonSkill(skill, new Percent(1)));
			agent.AddPersonPeriod(personPeriod);

			OvertimeAvailabilityRepository.Add(new OvertimeAvailability(agent, new DateOnly(2017, 1, 11), TimeSpan.FromHours(12), TimeSpan.FromHours(20)));

			SkillDayRepository.Has(skill.CreateSkillDaysWithDemandOnConsecutiveDays(scenario, new DateOnly(period.StartDateTime), 5));
			var ruleSetBag =
				new RuleSetBag(
					new WorkShiftRuleSet(new WorkShiftTemplateGenerator(activity, new TimePeriodWithSegment(10, 0, 16, 0, 60),
						new TimePeriodWithSegment(16, 0, 22, 0, 60), category))).WithId();

			RuleSetBagRepository.Add(ruleSetBag);

			Target.Handle(new AddOverTimeEvent
			{
				OvertimeDurationMin = TimeSpan.FromHours(1),
				OvertimeDurationMax = TimeSpan.FromHours(3),
				OvertimeType = multi.Id.Value,
				Skills = new[] {skill.Id.GetValueOrDefault()},
				ShiftBagToUse = ruleSetBag.Id
			});
			var days = SchedulingResultStateHolder().Schedules[agent];
			var overtimes = days.ScheduledDay(new DateOnly(2017, 1, 11)).PersonAssignment().OvertimeActivities();
			overtimes.Should().Not.Be.Empty();

		}
	}
}
