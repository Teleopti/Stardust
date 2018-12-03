using System;
using System.Collections.Generic;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.AgentInfo;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.Assignment;
using Teleopti.Ccc.Domain.Scheduling.Legacy.Commands;
using Teleopti.Ccc.Domain.Scheduling.Overtime;
using Teleopti.Ccc.Domain.Scheduling.ScheduleTagging;
using Teleopti.Ccc.Domain.Scheduling.TimeLayer;
using Teleopti.Ccc.Domain.Security.Authentication;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.TestCommon.FakeRepositories;
using Teleopti.Ccc.TestCommon.IoC;
using Teleopti.Ccc.TestCommon.Scheduling;


namespace Teleopti.Ccc.DomainTest.Scheduling.Overtime
{
	[DomainTest]
	public class ScheduleOvertimeServiceTest
	{
		public ScheduleOvertimeService Target;
		public IResourceCalculation ResourceOptimizationHelper;
		public Func<ISchedulerStateHolder> SchedulerStateHolderFrom;
		public FakePersonAssignmentRepository PersonAssignmentRepository;
		public FakePersonRepository PersonRepository;
		public FakeSkillRepository SkillRepository;
		public FakeSkillDayRepository SkillDayRepository;
		public FakeScenarioRepository ScenarioRepository;
		public FakeTimeZoneGuard TimeZoneGuard;

		private IOvertimePreferences _overtimePreferences;
		private DateOnly _dateOnly;
		private IPerson _person;
		private IPersonPeriod _personPeriod;
		private ISkill _skill;
		private IActivity _activity;
		private IMultiplicatorDefinitionSet _multiplicatorDefinitionSet;

		[SetUp]
		public void SetUp()
		{
			_overtimePreferences = new OvertimePreferences();
			_dateOnly = new DateOnly(2014, 1, 1);
			_person = PersonFactory.CreatePerson("person");
			_activity = ActivityFactory.CreateActivity("activity");
			_skill = SkillFactory.CreateSkill("skill");
			_skill.Activity = _activity;
			_personPeriod = PersonPeriodFactory.CreatePersonPeriodWithSkills(_dateOnly, new[] {_skill});
			_person.AddPersonPeriod(_personPeriod);
			_overtimePreferences.SkillActivity = _activity;
			_overtimePreferences.SelectedSpecificTimePeriod = new TimePeriod(TimeSpan.Zero, new TimeSpan(1, 6, 0, 0));
			_multiplicatorDefinitionSet = MultiplicatorDefinitionSetFactory.CreateMultiplicatorDefinitionSet("overtime", MultiplicatorType.Overtime);
			_personPeriod.PersonContract.Contract.AddMultiplicatorDefinitionSetCollection(_multiplicatorDefinitionSet);
		}

		[Test]
		public void ShouldOnlyScheduleOverTimeOnSelectedTimePeriodInViewPointTimeZoneBug39740()
		{
			var definitionSet = new MultiplicatorDefinitionSet("overtime", MultiplicatorType.Overtime);
			var phoneActivity = ActivityFactory.CreateActivity("phone");
			var skill = SkillRepository.Has("skill", phoneActivity);
			skill.TimeZone = TimeZoneInfoFactory.StockholmTimeZoneInfo();
			var dateOnly = new DateOnly(2016, 7, 12);
			var scenario = ScenarioRepository.Has("some name");
			var schedulePeriod = new SchedulePeriod(dateOnly, SchedulePeriodType.Week, 1);
			var worktimeDirective = new WorkTimeDirective(TimeSpan.FromHours(36), TimeSpan.FromHours(63), TimeSpan.FromHours(11), TimeSpan.FromHours(36));
			var contract = new Contract("contract") { WorkTimeDirective = worktimeDirective, PositivePeriodWorkTimeTolerance = TimeSpan.FromHours(9) };
			contract.AddMultiplicatorDefinitionSetCollection(definitionSet);
			var agent = PersonRepository.Has(contract, new ContractSchedule("_"), new PartTimePercentage("_"), new Team { Site = new Site("site") }, schedulePeriod, skill);
			agent.PermissionInformation.SetDefaultTimeZone(TimeZoneInfoFactory.TaipeiTimeZoneInfo());
			var shiftCategory = new ShiftCategory("_").WithId();
			var skillDay = skill.CreateSkillDayWithDemand(scenario, dateOnly, TimeSpan.FromHours(10));
			var skillDay1 = skill.CreateSkillDayWithDemand(scenario, dateOnly.AddDays(1), TimeSpan.FromHours(10));
			var skillDay2 = skill.CreateSkillDayWithDemand(scenario, dateOnly.AddDays(2), TimeSpan.FromHours(10));
			SkillDayRepository.Has(new List<ISkillDay> { skillDay, skillDay1, skillDay2 });
			PersonAssignmentRepository.Has(agent, scenario, phoneActivity, shiftCategory, dateOnly.AddDays(1), new TimePeriod(10, 0, 11, 0));
			var ass = PersonAssignmentRepository.GetSingle(dateOnly.AddDays(1));
			var stateHolder = SchedulerStateHolderFrom.Fill(scenario, DateOnlyPeriod.CreateWithNumberOfWeeks(dateOnly, 1), new[] { agent }, new[] { ass }, new[] { skillDay, skillDay1, skillDay2 });
			var overtimePreference = new OvertimePreferences
			{
				OvertimeType = definitionSet,
				ScheduleTag = new ScheduleTag(),
				SelectedSpecificTimePeriod = new TimePeriod(TimeSpan.Zero, new TimeSpan(1, 6, 0, 0)),
				SelectedTimePeriod = new TimePeriod(1, 0, 1, 0),
				SkillActivity = phoneActivity
			};
			var resourceCalculateDelayer = new ResourceCalculateDelayer(ResourceOptimizationHelper, true, stateHolder.SchedulingResultState, UserTimeZone.Make());
			var scheduleTagSetter = new ScheduleTagSetter(overtimePreference.ScheduleTag);
			TimeZoneGuard.SetTimeZone(TimeZoneInfoFactory.DenverTimeZoneInfo());
			Target.SchedulePersonOnDay(stateHolder.Schedules[agent], overtimePreference,
				resourceCalculateDelayer, dateOnly.AddDays(1), scheduleTagSetter);

			stateHolder.Schedules[agent].ScheduledDay(dateOnly.AddDays(1)).PersonAssignment(true).OvertimeActivities().Should().Be.Empty();
		}
	}
}
