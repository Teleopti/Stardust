using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Common.Time;
using Teleopti.Ccc.Domain.Helper;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.Assignment;
using Teleopti.Ccc.Domain.Scheduling.Legacy.Commands;
using Teleopti.Ccc.Domain.Scheduling.Overtime;
using Teleopti.Ccc.Domain.Scheduling.ScheduleTagging;
using Teleopti.Ccc.Domain.Scheduling.TimeLayer;
using Teleopti.Ccc.Domain.Staffing;
using Teleopti.Ccc.Infrastructure.Intraday;
using Teleopti.Ccc.IocCommon;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.TestCommon.FakeRepositories;
using Teleopti.Ccc.TestCommon.IoC;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.Scheduling.Overtime
{
	[DomainTest]
	[AllTogglesOn]
	public class ScheduleOvertimeExecuteWrapperTest : ISetup
	{
		public ScheduleOvertimeExecuteWrapper Target;
		public FakePersonAssignmentRepository PersonAssignmentRepository;
		public FakeSkillDayRepository SkillDayRepository;
		public IScheduleStorage ScheduleStorage;
		public FakeScenarioRepository ScenarioRepository;
		public FakeActivityRepository ActivityRepository;
		public FakeSkillRepository SkillRepository;
		public FakePersonRepository PersonRepository;
		public ISkillCombinationResourceRepository SkillCombinationResourceRepository;
		public MutableNow Now;
		public FakeUserTimeZone UserTimeZone;
		public AddOverTime Target2;
		public FakePersonForOvertimeProvider FakePersonForOvertimeProvider;
		private IScenario scenario;
		private IActivity activity;
		private ISkill skill;
		private IMultiplicatorDefinitionSet multiplicatorDefinitionSet;
		private IShiftCategory shiftCategory;
		private IContract contract;

		public void Setup(ISystem system, IIocConfiguration configuration)
		{
			system.UseTestDouble<ScheduleOvertimeExecuteWrapper>().For<ScheduleOvertimeExecuteWrapper>();
			system.UseTestDouble<ScheduleOvertimeServiceWithoutStateholder>().For<IScheduleOvertimeServiceWithoutStateholder>();
			system.UseTestDouble<ScheduleOvertimeWithoutStateHolder>().For<ScheduleOvertimeWithoutStateHolder>();
			system.UseTestDouble<GridlockManager>().For<IGridlockManager>();
			system.UseTestDouble<FakeUserTimeZone>().For<IUserTimeZone>();
			system.UseTestDouble<FakePersonForOvertimeProvider>().For<IPersonForOvertimeProvider>();
			system.UseTestDouble<AddOverTime>().For<IAddOverTime>();
		}

		private void setup()
		{
			scenario = ScenarioRepository.Has("scenario");
			activity = ActivityRepository.Has("activity");
			skill = SkillRepository.Has("skill", activity);
			skill.DefaultResolution = 60;
			shiftCategory = new ShiftCategory("shiftCategory").WithId();
			multiplicatorDefinitionSet = new MultiplicatorDefinitionSet("overtime", MultiplicatorType.Overtime);
			contract = new Contract("contract");
			contract.AddMultiplicatorDefinitionSetCollection(multiplicatorDefinitionSet);

			Now.Is("2017-06-01 08:00");
		}

		[Test]
		public void ShouldScheduleOvertimeOnAgent()
		{
			setup();
			SkillCombinationResourceRepository.PersistSkillCombinationResource(Now.UtcDateTime(), new[]
			{
				new SkillCombinationResource
				{
					StartDateTime = new DateTime(2017, 06, 1, 15, 0, 0).Utc(),
					EndDateTime = new DateTime(2017, 06, 01, 16, 0, 0).Utc(),
					Resource = 1,
					SkillCombination = new[] {skill.Id.GetValueOrDefault()}
				}
			});

			var dateOnly = new DateOnly(2017, 06, 1);
			var agent = new Person().WithId().InTimeZone(TimeZoneInfo.Utc).WithPersonPeriod(contract, skill).WithSchedulePeriodOneWeek(dateOnly);
			PersonRepository.Has(agent);

			FakePersonForOvertimeProvider.Fill(new List<SuggestedPersonsModel>{new SuggestedPersonsModel{PersonId = agent.Id.GetValueOrDefault()}});
			SkillDayRepository.Has(skill.CreateSkillDayWithDemand(scenario, dateOnly, 1));
			var ass = new PersonAssignment(agent, scenario, dateOnly).ShiftCategory(shiftCategory).WithLayer(activity, new TimePeriod(15, 16));
			PersonAssignmentRepository.Has(ass);
			var overtimePreference = new OvertimePreferences
			{
				OvertimeType = multiplicatorDefinitionSet,
				ScheduleTag = new NullScheduleTag(),
				SelectedSpecificTimePeriod = new TimePeriod(16, 0, 17, 0),
				SelectedTimePeriod = new TimePeriod(1, 0, 1, 0),
				SkillActivity = activity
			};
			
			var model = new OverTimeSuggestionModel
			{
				SkillIds = new[] {skill.Id.GetValueOrDefault()},
				TimeSerie = new[] {new DateTime(2017, 06, 1, 0, 0, 0), new DateTime(2017, 06, 2, 0, 0, 0)},
				OvertimePreferences = overtimePreference
			};

			var result = Target2.GetSuggestion(model);
			
			result.Models.Should().Not.Be.Empty();
		}

		[Test]
		public void ShouldScheduleOvertimeOnOnlyOneAgentWhenMoreAreAvailable()
		{
			setup();
			SkillCombinationResourceRepository.PersistSkillCombinationResource(Now.UtcDateTime(), new[]
			{
				new SkillCombinationResource
				{
					StartDateTime = new DateTime(2017, 06, 1, 15, 0, 0).Utc(),
					EndDateTime = new DateTime(2017, 06, 01, 16, 0, 0).Utc(),
					Resource = 2,
					SkillCombination = new[] {skill.Id.GetValueOrDefault()}
				}
			});

			var dateOnly = new DateOnly(2017, 06, 1);

			var agent = new Person().WithId().InTimeZone(TimeZoneInfo.Utc).WithPersonPeriod(contract, skill).WithSchedulePeriodOneWeek(dateOnly);
			var ass = new PersonAssignment(agent, scenario, dateOnly).ShiftCategory(shiftCategory).WithLayer(activity, new TimePeriod(15, 16));
			PersonAssignmentRepository.Has(ass);

			var agent2 = new Person().WithId().InTimeZone(TimeZoneInfo.Utc).WithPersonPeriod(contract, skill).WithSchedulePeriodOneWeek(dateOnly);
			var ass2 = new PersonAssignment(agent2, scenario, dateOnly).ShiftCategory(shiftCategory).WithLayer(activity, new TimePeriod(15, 16));
			PersonAssignmentRepository.Has(ass2);

			SkillDayRepository.Has(skill.CreateSkillDayWithDemand(scenario, dateOnly, 1));
			var overtimePreference = new OvertimePreferences
			{
				OvertimeType = multiplicatorDefinitionSet,
				ScheduleTag = new NullScheduleTag(),
				SelectedSpecificTimePeriod = new TimePeriod(16, 0, 17, 0),
				SelectedTimePeriod = new TimePeriod(TimeSpan.FromMinutes(1), TimeSpan.FromHours(1)),
				SkillActivity = activity
			};

			var requestedPeriod = new DateTimePeriod(2017, 06, 1, 16, 2017, 06, 1, 17);
			var fullPeriod = requestedPeriod;
			var dateTimePeriod = dateOnly.ToDateTimePeriod(TimeZoneInfo.Utc);
			var scheduleDictionary = ScheduleStorage.FindSchedulesForPersons(new ScheduleDateTimePeriod(dateTimePeriod), scenario, new PersonProvider(new[] {agent, agent2}), new ScheduleDictionaryLoadOptions(false, false), new[] {agent, agent2});
			var result = Target.Execute(overtimePreference, new NoSchedulingProgress(), scheduleDictionary, new[]{agent, agent2}, dateOnly.ToDateOnlyPeriod(), requestedPeriod, new[] {skill}, new[] { skill }, fullPeriod);
			var overtimeActivities = scheduleDictionary.SchedulesForDay(dateOnly).ToList()
				.Select(x => x.PersonAssignment().OvertimeActivities())
				.SelectMany(i => i).Where(ot => ot.Period == new DateTimePeriod(2017, 06, 01, 16, 2017, 06, 01, 17));
			overtimeActivities.Count().Should().Be.EqualTo(1);
			result.Models.Count.Should().Be.EqualTo(1);
		}

		[Test]
		public void ShouldScheduleOvertimeOnDifferentPeriods()
		{
			setup();
			skill.StaffingThresholds = new StaffingThresholds(skill.StaffingThresholds.SeriousUnderstaffing, skill.StaffingThresholds.Understaffing, new Percent(0));
			SkillCombinationResourceRepository.PersistSkillCombinationResource(Now.UtcDateTime(), new[]
			{
				new SkillCombinationResource
				{
					StartDateTime = new DateTime(2017, 06, 1, 15, 0, 0).Utc(),
					EndDateTime = new DateTime(2017, 06, 01, 16, 0, 0).Utc(),
					Resource = 2,
					SkillCombination = new[] {skill.Id.GetValueOrDefault()}
				},
				new SkillCombinationResource
				{
					StartDateTime = new DateTime(2017, 06, 1, 16, 0, 0).Utc(),
					EndDateTime = new DateTime(2017, 06, 01, 17, 0, 0).Utc(),
					Resource = 1,
					SkillCombination = new[] {skill.Id.GetValueOrDefault()}
				}
			});

			var dateOnly = new DateOnly(2017, 06, 1);

			var agent = new Person().WithId().InTimeZone(TimeZoneInfo.Utc).WithPersonPeriod(contract, skill).WithSchedulePeriodOneWeek(dateOnly);
			var ass = new PersonAssignment(agent, scenario, dateOnly).ShiftCategory(shiftCategory).WithLayer(activity, new TimePeriod(15, 16));
			PersonAssignmentRepository.Has(ass);

			var agent2 = new Person().WithId().InTimeZone(TimeZoneInfo.Utc).WithPersonPeriod(contract, skill).WithSchedulePeriodOneWeek(dateOnly);
			var ass2 = new PersonAssignment(agent2, scenario, dateOnly).ShiftCategory(shiftCategory).WithLayer(activity, new TimePeriod(15, 17));
			PersonAssignmentRepository.Has(ass2);

			SkillDayRepository.Has(skill.CreateSkillDayWithDemand(scenario, dateOnly, 2));
			var overtimePreference = new OvertimePreferences
			{
				OvertimeType = multiplicatorDefinitionSet,
				ScheduleTag = new NullScheduleTag(),
				SelectedSpecificTimePeriod = new TimePeriod(16, 0, 19, 0),
				SelectedTimePeriod = new TimePeriod(1, 0, 3, 0),
				SkillActivity = activity
			};

			var requestedPeriod = new DateTimePeriod(2017, 06, 1, 16, 2017, 06, 1, 19);
			var fullPeriod = requestedPeriod;
			var dateTimePeriod = dateOnly.ToDateTimePeriod(TimeZoneInfo.Utc);
			var scheduleDictionary = ScheduleStorage.FindSchedulesForPersons(new ScheduleDateTimePeriod(dateTimePeriod), scenario, new PersonProvider(new[] { agent, agent2 }), new ScheduleDictionaryLoadOptions(false, false), new[] { agent, agent2 });
			var result = Target.Execute(overtimePreference, new NoSchedulingProgress(), scheduleDictionary, new[] { agent, agent2 }, dateOnly.ToDateOnlyPeriod(), requestedPeriod, new[] { skill }, new[] { skill }, fullPeriod);
			var overtimeActivities = scheduleDictionary.SchedulesForDay(dateOnly).ToList()
				.Select(x => x.PersonAssignment().OvertimeActivities())
				.SelectMany(i => i).Where(ot => ot.Period == new DateTimePeriod(2017, 06, 01, 16, 2017, 06, 01, 19));
			var overtimeActivities2 = scheduleDictionary.SchedulesForDay(dateOnly).ToList()
				.Select(x => x.PersonAssignment().OvertimeActivities())
				.SelectMany(i => i).Where(ot => ot.Period == new DateTimePeriod(2017, 06, 01, 17, 2017, 06, 01, 19));

			overtimeActivities.Count().Should().Be.EqualTo(1);
			overtimeActivities2.Count().Should().Be.EqualTo(1);
			result.Models.Count.Should().Be.EqualTo(2);
		}

		[Test]
		public void ShouldNotScheduleOvertimeOnSecondAgentDueToOverStaffing()
		{
			setup();
			skill.StaffingThresholds = new StaffingThresholds(skill.StaffingThresholds.SeriousUnderstaffing, skill.StaffingThresholds.Understaffing, new Percent(0));
			SkillCombinationResourceRepository.PersistSkillCombinationResource(Now.UtcDateTime(), new[]
			{
				new SkillCombinationResource
				{
					StartDateTime = new DateTime(2017, 06, 1, 15, 0, 0).Utc(),
					EndDateTime = new DateTime(2017, 06, 01, 16, 0, 0).Utc(),
					Resource = 2,
					SkillCombination = new[] {skill.Id.GetValueOrDefault()}
				},
				new SkillCombinationResource
				{
					StartDateTime = new DateTime(2017, 06, 1, 16, 0, 0).Utc(),
					EndDateTime = new DateTime(2017, 06, 01, 17, 0, 0).Utc(),
					Resource = 1,
					SkillCombination = new[] {skill.Id.GetValueOrDefault()}
				},
				new SkillCombinationResource
				{
					StartDateTime = new DateTime(2017, 06, 1, 17, 0, 0).Utc(),
					EndDateTime = new DateTime(2017, 06, 01, 18, 0, 0).Utc(),
					Resource = 1,
					SkillCombination = new[] {skill.Id.GetValueOrDefault()}
				},
				new SkillCombinationResource
				{
					StartDateTime = new DateTime(2017, 06, 1, 18, 0, 0).Utc(),
					EndDateTime = new DateTime(2017, 06, 01, 19, 0, 0).Utc(),
					Resource = 1,
					SkillCombination = new[] {skill.Id.GetValueOrDefault()}
				}
			});

			var dateOnly = new DateOnly(2017, 06, 1);

			var agent = new Person().WithId().InTimeZone(TimeZoneInfo.Utc).WithPersonPeriod(contract, skill).WithSchedulePeriodOneWeek(dateOnly);
			var ass = new PersonAssignment(agent, scenario, dateOnly).ShiftCategory(shiftCategory).WithLayer(activity, new TimePeriod(15, 16));
			PersonAssignmentRepository.Has(ass);

			var agent2 = new Person().WithId().InTimeZone(TimeZoneInfo.Utc).WithPersonPeriod(contract, skill).WithSchedulePeriodOneWeek(dateOnly);
			var ass2 = new PersonAssignment(agent2, scenario, dateOnly).ShiftCategory(shiftCategory).WithLayer(activity, new TimePeriod(15, 16));
			PersonAssignmentRepository.Has(ass2);

			SkillDayRepository.Has(skill.CreateSkillDayWithDemand(scenario, dateOnly, 2));
			var overtimePreference = new OvertimePreferences
			{
				OvertimeType = multiplicatorDefinitionSet,
				ScheduleTag = new NullScheduleTag(),
				SelectedSpecificTimePeriod = new TimePeriod(16, 0, 20, 0),
				SelectedTimePeriod = new TimePeriod(1, 0, 4, 0),
				SkillActivity = activity
			};

			var requestedPeriod = new DateTimePeriod(2017, 06, 1, 16, 2017, 06, 1, 20);
			var fullPeriod = requestedPeriod;
			var dateTimePeriod = dateOnly.ToDateTimePeriod(TimeZoneInfo.Utc);
			var scheduleDictionary = ScheduleStorage.FindSchedulesForPersons(new ScheduleDateTimePeriod(dateTimePeriod), scenario, new PersonProvider(new[] { agent, agent2 }), new ScheduleDictionaryLoadOptions(false, false), new[] { agent, agent2 });
			var result = Target.Execute(overtimePreference, new NoSchedulingProgress(), scheduleDictionary, new[] { agent, agent2 }, dateOnly.ToDateOnlyPeriod(), requestedPeriod, new[] { skill }, new[] { skill }, fullPeriod);
			var overtimeActivities = scheduleDictionary.SchedulesForDay(dateOnly).ToList()
				.Select(x => x.PersonAssignment().OvertimeActivities())
				.SelectMany(i => i).Where(ot => ot.Period == new DateTimePeriod(2017, 06, 01, 16, 2017, 06, 01, 20));
			
			overtimeActivities.Count().Should().Be.EqualTo(1);
			result.Models.Count.Should().Be.EqualTo(1);
		}

		[Test]
		public void ShouldScheduleOvertimeOnDifferentTimezones()
		{
			setup();
			skill.StaffingThresholds = new StaffingThresholds(skill.StaffingThresholds.SeriousUnderstaffing, skill.StaffingThresholds.Understaffing, new Percent(0));
			SkillCombinationResourceRepository.PersistSkillCombinationResource(Now.UtcDateTime(), new[]
			{
				new SkillCombinationResource
				{
					StartDateTime = new DateTime(2017, 06, 01, 5, 0, 0).Utc(),
					EndDateTime = new DateTime(2017, 06, 01, 6, 0, 0).Utc(),
					Resource = 2,
					SkillCombination = new[] {skill.Id.GetValueOrDefault()}
				},
				new SkillCombinationResource
				{
					StartDateTime = new DateTime(2017, 06, 01, 6, 0, 0).Utc(),
					EndDateTime = new DateTime(2017, 06, 01, 7, 0, 0).Utc(),
					Resource = 1,
					SkillCombination = new[] {skill.Id.GetValueOrDefault()}
				}
			});

			var june1DateOnly = new DateOnly(2017, 06, 1);
			var may31DateOnly = new DateOnly(2017, 05, 31);

			// Singapore Standard Time +8H
			var agent = new Person().WithId().InTimeZone(TimeZoneInfo.FindSystemTimeZoneById("Singapore Standard Time")).WithPersonPeriod(contract, skill).WithSchedulePeriodOneWeek(june1DateOnly);
			var timePeriodInUtc = new DateTimePeriod(2017, 06, 01, 5, 2017, 06, 01, 6);

			var ass = new PersonAssignment(agent, scenario, june1DateOnly).ShiftCategory(shiftCategory).WithLayer(activity, timePeriodInUtc);
			PersonAssignmentRepository.Has(ass);

			// Mountain Standard Time -7H
			var agent2 = new Person().WithId().InTimeZone(TimeZoneInfo.FindSystemTimeZoneById("Mountain Standard Time")).WithPersonPeriod(contract, skill).WithSchedulePeriodOneWeek(june1DateOnly);
			var ass2 = new PersonAssignment(agent2, scenario, may31DateOnly).ShiftCategory(shiftCategory).WithLayer(activity, timePeriodInUtc);
			PersonAssignmentRepository.Has(ass2);

			SkillDayRepository.Has(skill.CreateSkillDayWithDemand(scenario, may31DateOnly, 2));
			SkillDayRepository.Has(skill.CreateSkillDayWithDemand(scenario, june1DateOnly, 2));

			var overtimePreference = new OvertimePreferences
			{
				OvertimeType = multiplicatorDefinitionSet,
				ScheduleTag = new NullScheduleTag(),
				SelectedSpecificTimePeriod = new TimePeriod(6, 0, 10, 0),
				SelectedTimePeriod = new TimePeriod(1, 0, 4, 0),
				SkillActivity = activity
			};

			var requestedPeriod = new DateTimePeriod(2017, 06, 1, 6, 2017, 06, 1, 10);
			var fullPeriod = requestedPeriod;
			var dateTimePeriod = june1DateOnly.ToDateTimePeriod(TimeZoneInfo.Utc);
			var scheduleDictionary = ScheduleStorage.FindSchedulesForPersons(new ScheduleDateTimePeriod(dateTimePeriod), scenario, new PersonProvider(new[] { agent, agent2 }), new ScheduleDictionaryLoadOptions(false, false), new[] { agent, agent2 });

			var dateOnlyPeriod = new DateOnlyPeriod(may31DateOnly, june1DateOnly);
			var result = Target.Execute(overtimePreference, new NoSchedulingProgress(), scheduleDictionary, new[]{agent, agent2}, dateOnlyPeriod, requestedPeriod, new[] { skill }, new[] { skill }, fullPeriod);
			var overtimeActivities = scheduleDictionary.SchedulesForPeriod(dateOnlyPeriod, agent, agent2).ToList()
				.Select(x => x.PersonAssignment()?.OvertimeActivities())
				.Where(y => y != null)
				.SelectMany(i => i).Where(ot => ot.Period == new DateTimePeriod(2017, 06, 01, 6, 2017, 06, 01, 10));

			overtimeActivities.Count().Should().Be.EqualTo(2);
			result.Models.Count.Should().Be.EqualTo(2);
		}

		[Test]
		public void ShouldScheduleOvertimeBeforeShift()
		{
			setup();
			skill.StaffingThresholds = new StaffingThresholds(skill.StaffingThresholds.SeriousUnderstaffing, skill.StaffingThresholds.Understaffing, new Percent(0));
			SkillCombinationResourceRepository.PersistSkillCombinationResource(Now.UtcDateTime(), new[]
			{
				new SkillCombinationResource
				{
					StartDateTime = new DateTime(2017, 06, 01, 6, 0, 0).Utc(),
					EndDateTime = new DateTime(2017, 06, 01, 7, 0, 0).Utc(),
					Resource = 1,
					SkillCombination = new[] {skill.Id.GetValueOrDefault()}
				}
			});

			var dateOnly = new DateOnly(2017, 06, 1);

			var agent = new Person().WithId().InTimeZone(TimeZoneInfo.Utc).WithPersonPeriod(contract, skill).WithSchedulePeriodOneWeek(dateOnly);
			var timePeriodInUtc = new DateTimePeriod(2017, 06, 01, 6, 2017, 06, 01, 7);

			var ass = new PersonAssignment(agent, scenario, dateOnly).ShiftCategory(shiftCategory).WithLayer(activity, timePeriodInUtc);
			PersonAssignmentRepository.Has(ass);

			SkillDayRepository.Has(skill.CreateSkillDayWithDemand(scenario, dateOnly, 2));
			var overtimePreference = new OvertimePreferences
			{
				OvertimeType = multiplicatorDefinitionSet,
				ScheduleTag = new NullScheduleTag(),
				SelectedSpecificTimePeriod = new TimePeriod(1, 0, 7, 0),
				SelectedTimePeriod = new TimePeriod(1, 0, 4, 0),
				SkillActivity = activity
			};

			var requestedPeriod = new DateTimePeriod(2017, 06, 1, 1, 2017, 06, 1, 7);
			var fullPeriod = requestedPeriod;
			var dateTimePeriod = dateOnly.ToDateTimePeriod(TimeZoneInfo.Utc);
			var scheduleDictionary = ScheduleStorage.FindSchedulesForPersons(new ScheduleDateTimePeriod(dateTimePeriod), scenario, new PersonProvider(new[] { agent }), new ScheduleDictionaryLoadOptions(false, false), new[] { agent });
			var period = new DateOnlyPeriod(dateOnly.AddDays(-1), dateOnly.AddDays(1));
			var result = Target.Execute(overtimePreference, new NoSchedulingProgress(), scheduleDictionary, new[] { agent }, period, requestedPeriod, new[] { skill }, new[] { skill }, fullPeriod);
			var overtimeActivities = scheduleDictionary.SchedulesForPeriod(period, agent).ToList()
				.Select(x => x.PersonAssignment()?.OvertimeActivities())
				.Where(y => y != null)
				.SelectMany(i => i).Where(ot => ot.Period == new DateTimePeriod(2017, 06, 01, 2, 2017, 06, 01, 6));

			overtimeActivities.Count().Should().Be.EqualTo(1);
			result.Models.Count.Should().Be.EqualTo(1);
		}

		[Test]
		public void ShouldScheduleOvertimeBeforeShiftOnDifferentTimezones()
		{
			setup();
			Now.Is("2017-05-31 08:00");
			skill.StaffingThresholds = new StaffingThresholds(skill.StaffingThresholds.SeriousUnderstaffing, skill.StaffingThresholds.Understaffing, new Percent(0));
			SkillCombinationResourceRepository.PersistSkillCombinationResource(Now.UtcDateTime(), new[]
			{
				new SkillCombinationResource
				{
					StartDateTime = new DateTime(2017, 06, 01, 5, 0, 0).Utc(),
					EndDateTime = new DateTime(2017, 06, 01, 6, 0, 0).Utc(),
					Resource = 2,
					SkillCombination = new[] {skill.Id.GetValueOrDefault()}
				}
			});

			var june1DateOnly = new DateOnly(2017, 06, 1);
			var may31DateOnly = new DateOnly(2017, 05, 31);

			// Singapore Standard Time +8H
			var agent = new Person().WithId().InTimeZone(TimeZoneInfo.FindSystemTimeZoneById("Singapore Standard Time")).WithPersonPeriod(contract, skill).WithSchedulePeriodOneWeek(june1DateOnly);
			PersonRepository.Has(agent);
			var timePeriodInUtc = new DateTimePeriod(2017, 06, 01, 5, 2017, 06, 01, 6);

			var ass = new PersonAssignment(agent, scenario, june1DateOnly).ShiftCategory(shiftCategory).WithLayer(activity, timePeriodInUtc);
			PersonAssignmentRepository.Has(ass);

			// Mountain Standard Time -7H
			var agent2 = new Person().WithId().InTimeZone(TimeZoneInfo.FindSystemTimeZoneById("Mountain Standard Time")).WithPersonPeriod(contract, skill).WithSchedulePeriodOneWeek(june1DateOnly);
			PersonRepository.Has(agent2);
			FakePersonForOvertimeProvider.Fill(new List<SuggestedPersonsModel> { new SuggestedPersonsModel { PersonId = agent.Id.GetValueOrDefault() }, new SuggestedPersonsModel { PersonId = agent2.Id.GetValueOrDefault() } });

			var ass2 = new PersonAssignment(agent2, scenario, may31DateOnly).ShiftCategory(shiftCategory).WithLayer(activity, timePeriodInUtc);
			PersonAssignmentRepository.Has(ass2);

			SkillDayRepository.Has(skill.CreateSkillDayWithDemand(scenario, june1DateOnly, 2));
			var overtimePreference = new OvertimePreferences
			{
				OvertimeType = multiplicatorDefinitionSet,
				ScheduleTag = new NullScheduleTag(),
				SelectedSpecificTimePeriod = new TimePeriod(1, 0, 5, 0),
				SelectedTimePeriod = new TimePeriod(1, 0, 4, 0),
				SkillActivity = activity
			};

			var model = new OverTimeSuggestionModel
			{
				SkillIds = new[] { skill.Id.GetValueOrDefault() },
				TimeSerie = new[] { new DateTime(2017, 06, 1, 0, 0, 0), new DateTime(2017, 06, 2, 0, 0, 0) },
				OvertimePreferences = overtimePreference
			};

			var result = Target2.GetSuggestion(model);

			result.Models.Count.Should().Be.EqualTo(2);
		}


		[Test]
		public void ShouldScheduleOvertimeOnAgentOnPeriodOverUtcMidnight()
		{
			setup();
			UserTimeZone.Is(TimeZoneInfo.FindSystemTimeZoneById("Mountain Standard Time"));
			SkillCombinationResourceRepository.PersistSkillCombinationResource(Now.UtcDateTime(), new[]
			{
				new SkillCombinationResource
				{
					StartDateTime = new DateTime(2017, 06, 1, 23, 0, 0).Utc(),
					EndDateTime = new DateTime(2017, 06, 2, 00, 0, 0).Utc(),
					Resource = 1,
					SkillCombination = new[] {skill.Id.GetValueOrDefault()}
				}
			});

			var june1DateOnly = new DateOnly(2017, 06, 1);
			var june2DateOnly = new DateOnly(2017, 06, 2);
			var agent = new Person().WithId().InTimeZone(TimeZoneInfo.Utc).WithPersonPeriod(contract, skill).WithSchedulePeriodOneWeek(june1DateOnly);
			SkillDayRepository.Has(skill.CreateSkillDayWithDemand(scenario, june1DateOnly, 1));
			SkillDayRepository.Has(skill.CreateSkillDayWithDemand(scenario, june2DateOnly, 1));
			var ass = new PersonAssignment(agent, scenario, june1DateOnly).ShiftCategory(shiftCategory).WithLayer(activity, new TimePeriod(23, 24));
			PersonAssignmentRepository.Has(ass);
			var overtimePreference = new OvertimePreferences
			{
				OvertimeType = multiplicatorDefinitionSet,
				ScheduleTag = new NullScheduleTag(),
				SelectedSpecificTimePeriod = new TimePeriod(15, 0, 22, 0),
				SelectedTimePeriod = new TimePeriod(1, 0, 4, 0),
				SkillActivity = activity
			};

			var requestedPeriod = new DateTimePeriod(2017, 06, 1, 21, 2017, 06, 2, 4);
			var fullPeriod = requestedPeriod;
			var dateTimePeriod = june1DateOnly.ToDateTimePeriod(TimeZoneInfo.Utc);
			var scheduleDictionary = ScheduleStorage.FindSchedulesForPersons(new ScheduleDateTimePeriod(dateTimePeriod), scenario, new PersonProvider(new[] { agent }), new ScheduleDictionaryLoadOptions(false, false), new[] { agent });
			var result = Target.Execute(overtimePreference, new NoSchedulingProgress(), scheduleDictionary, new[]{agent}, june1DateOnly.ToDateOnlyPeriod(), requestedPeriod, new[] { skill },new[] { skill }, fullPeriod);
			var overtimeActivities = scheduleDictionary[agent].ScheduledDay(june1DateOnly).PersonAssignment().OvertimeActivities().Where(ot => ot.Period == new DateTimePeriod(2017, 06, 2, 0, 2017, 06, 2, 4));
			overtimeActivities.Count().Should().Be.EqualTo(1);
			result.Models.Should().Not.Be.Empty();
		}

		[Test]
		public void ShouldScheduleOvertimeOnAgentOverMidnight()
		{
			setup();
			SkillCombinationResourceRepository.PersistSkillCombinationResource(Now.UtcDateTime(), new[]
			{
				new SkillCombinationResource
				{
					StartDateTime = new DateTime(2017, 06, 1, 22, 0, 0).Utc(),
					EndDateTime = new DateTime(2017, 06, 1, 23, 0, 0).Utc(),
					Resource = 1,
					SkillCombination = new[] {skill.Id.GetValueOrDefault()}
				}
			});

			var june1DateOnly = new DateOnly(2017, 06, 1);
			var june2DateOnly = new DateOnly(2017, 06, 2);
			var agent = new Person().WithId().InTimeZone(TimeZoneInfo.Utc).WithPersonPeriod(contract, skill).WithSchedulePeriodOneWeek(june1DateOnly);
			SkillDayRepository.Has(skill.CreateSkillDayWithDemand(scenario, june1DateOnly, 1));
			SkillDayRepository.Has(skill.CreateSkillDayWithDemand(scenario, june2DateOnly, 1));
			var ass = new PersonAssignment(agent, scenario, june1DateOnly).ShiftCategory(shiftCategory).WithLayer(activity, new TimePeriod(22, 23));
			PersonAssignmentRepository.Has(ass);
			var overtimePreference = new OvertimePreferences
			{
				OvertimeType = multiplicatorDefinitionSet,
				ScheduleTag = new NullScheduleTag(),
				SelectedTimePeriod = new TimePeriod(1, 0, 4, 0),
				SkillActivity = activity
			};

			var requestedPeriod = new DateTimePeriod(2017, 06, 1, 22, 2017, 06, 2, 4);
			var fullPeriod = requestedPeriod;
			var dateTimePeriod = june1DateOnly.ToDateTimePeriod(TimeZoneInfo.Utc);
			var scheduleDictionary = ScheduleStorage.FindSchedulesForPersons(new ScheduleDateTimePeriod(dateTimePeriod), scenario, new PersonProvider(new[] { agent }), new ScheduleDictionaryLoadOptions(false, false), new[] { agent });
			var result = Target.Execute(overtimePreference, new NoSchedulingProgress(), scheduleDictionary, new[] { agent }, june1DateOnly.ToDateOnlyPeriod(), requestedPeriod, new[] { skill }, new[] { skill }, fullPeriod);
			var overtimeActivities = scheduleDictionary[agent].ScheduledDay(june1DateOnly).PersonAssignment().OvertimeActivities().Where(ot => ot.Period == new DateTimePeriod(2017, 06, 1, 23, 2017, 06, 2, 3));
			overtimeActivities.Count().Should().Be.EqualTo(1);
			result.Models.Should().Not.Be.Empty();
		}

		[Test]
		public void ShouldScheduleOvertimeOnAgentInDenverOverUtcMidnight()
		{
			setup();
			SkillCombinationResourceRepository.PersistSkillCombinationResource(Now.UtcDateTime(), new[]
			{
				new SkillCombinationResource
				{
					StartDateTime = new DateTime(2017, 06, 1, 22, 0, 0).Utc(),
					EndDateTime = new DateTime(2017, 06, 1, 23, 0, 0).Utc(),
					Resource = 1,
					SkillCombination = new[] {skill.Id.GetValueOrDefault()}
				}
			});

			var june1DateOnly = new DateOnly(2017, 06, 1);
			var june2DateOnly = new DateOnly(2017, 06, 2);
			var agent = new Person().WithId().InTimeZone(TimeZoneInfo.FindSystemTimeZoneById("Mountain Standard Time")).WithPersonPeriod(contract, skill).WithSchedulePeriodOneWeek(june1DateOnly);
			SkillDayRepository.Has(skill.CreateSkillDayWithDemand(scenario, june1DateOnly, 1));
			SkillDayRepository.Has(skill.CreateSkillDayWithDemand(scenario, june2DateOnly, 1));
			var ass = new PersonAssignment(agent, scenario, june1DateOnly).ShiftCategory(shiftCategory).WithLayer(activity, new TimePeriod(16, 17));
			PersonAssignmentRepository.Has(ass);
			var overtimePreference = new OvertimePreferences
			{
				OvertimeType = multiplicatorDefinitionSet,
				ScheduleTag = new NullScheduleTag(),
				SelectedTimePeriod = new TimePeriod(1, 0, 4, 0),
				SkillActivity = activity
			};

			var requestedPeriod = new DateTimePeriod(2017, 06, 1, 22, 2017, 06, 2, 4);
			var fullPeriod = requestedPeriod;
			var dateTimePeriod = june1DateOnly.ToDateTimePeriod(TimeZoneInfo.Utc);
			var scheduleDictionary = ScheduleStorage.FindSchedulesForPersons(new ScheduleDateTimePeriod(dateTimePeriod), scenario, new PersonProvider(new[] { agent }), new ScheduleDictionaryLoadOptions(false, false), new[] { agent });
			var result = Target.Execute(overtimePreference, new NoSchedulingProgress(), scheduleDictionary, new[] { agent }, june1DateOnly.ToDateOnlyPeriod(), requestedPeriod, new[] { skill }, new[] { skill }, fullPeriod);
			var overtimeActivities = scheduleDictionary[agent].ScheduledDay(june1DateOnly).PersonAssignment().OvertimeActivities().Where(ot => ot.Period == new DateTimePeriod(2017, 06, 1, 23, 2017, 06, 2, 3));
			overtimeActivities.Count().Should().Be.EqualTo(1);
			result.Models.Should().Not.Be.Empty();
		}


		[Test]
		public void ShouldScheduleOvertimeOnAgentInDenverOverDenverMidnight()
		{
			setup();
			SkillCombinationResourceRepository.PersistSkillCombinationResource(Now.UtcDateTime(), new[]
			{
				new SkillCombinationResource
				{
					StartDateTime = new DateTime(2017, 06, 2, 4, 0, 0).Utc(),
					EndDateTime = new DateTime(2017, 06, 2, 5, 0, 0).Utc(),
					Resource = 1,
					SkillCombination = new[] {skill.Id.GetValueOrDefault()}
				}
			});

			var june1DateOnly = new DateOnly(2017, 06, 1);
			var june2DateOnly = new DateOnly(2017, 06, 2);
			var agent = new Person().WithId().InTimeZone(TimeZoneInfo.FindSystemTimeZoneById("Mountain Standard Time")).WithPersonPeriod(contract, skill).WithSchedulePeriodOneWeek(june1DateOnly);
			SkillDayRepository.Has(skill.CreateSkillDayWithDemand(scenario, june1DateOnly, 1));
			SkillDayRepository.Has(skill.CreateSkillDayWithDemand(scenario, june2DateOnly, 1));
			var ass = new PersonAssignment(agent, scenario, june1DateOnly).ShiftCategory(shiftCategory).WithLayer(activity, new TimePeriod(22, 23));
			PersonAssignmentRepository.Has(ass);
			var overtimePreference = new OvertimePreferences
			{
				OvertimeType = multiplicatorDefinitionSet,
				ScheduleTag = new NullScheduleTag(),
				SelectedTimePeriod = new TimePeriod(1, 0, 4, 0),
				SkillActivity = activity
			};

			var requestedPeriod = new DateTimePeriod(2017, 06, 2, 4, 2017, 06, 2, 9);
			var fullPeriod = requestedPeriod;
			var dateTimePeriod = june1DateOnly.ToDateTimePeriod(TimeZoneInfo.Utc);
			var scheduleDictionary = ScheduleStorage.FindSchedulesForPersons(new ScheduleDateTimePeriod(dateTimePeriod), scenario, new PersonProvider(new[] { agent }), new ScheduleDictionaryLoadOptions(false, false), new[] { agent });
			var result = Target.Execute(overtimePreference, new NoSchedulingProgress(), scheduleDictionary, new[] { agent }, june1DateOnly.ToDateOnlyPeriod(), requestedPeriod, new[] { skill }, new[] { skill }, fullPeriod);
			var overtimeActivities = scheduleDictionary[agent].ScheduledDay(june1DateOnly).PersonAssignment().OvertimeActivities().Where(ot => ot.Period == new DateTimePeriod(2017, 06, 2, 5, 2017, 06, 2, 9));
			overtimeActivities.Count().Should().Be.EqualTo(1);
			result.Models.Should().Not.Be.Empty();
		}


		[Test]
		public void ShouldNotScheduleOvertimeInThePastButReturnModelsForPast()
		{
			setup();
			Now.Is("2017-06-01 12:45");
			SkillCombinationResourceRepository.PersistSkillCombinationResource(Now.UtcDateTime(), new[]
			{
				new SkillCombinationResource
				{
					StartDateTime = new DateTime(2017, 06, 1, 15, 0, 0).Utc(),
					EndDateTime = new DateTime(2017, 06, 01, 16, 0, 0).Utc(),
					Resource = 1,
					SkillCombination = new[] {skill.Id.GetValueOrDefault()}
				}
			});

			var dateOnly = new DateOnly(2017, 06, 1);
			var agent = new Person().WithId().InTimeZone(TimeZoneInfo.Utc).WithPersonPeriod(contract, skill).WithSchedulePeriodOneWeek(dateOnly);
			SkillDayRepository.Has(skill.CreateSkillDayWithDemand(scenario, dateOnly, 1));
			var ass = new PersonAssignment(agent, scenario, dateOnly).ShiftCategory(shiftCategory).WithLayer(activity, new TimePeriod(15, 16));
			PersonAssignmentRepository.Has(ass);
			var overtimePreference = new OvertimePreferences
			{
				OvertimeType = multiplicatorDefinitionSet,
				ScheduleTag = new NullScheduleTag(),
				SelectedTimePeriod = new TimePeriod(1, 0, 4, 0),
				SkillActivity = activity
			};
			var requestedPeriod = new DateTimePeriod(2017, 06, 01, 13, 2017, 06, 01, 16);
			var fullPeriod = new DateTimePeriod(2017, 06, 01, 0, 2017, 06, 01, 16); 
			var dateTimePeriod = dateOnly.ToDateTimePeriod(TimeZoneInfo.Utc);
			var scheduleDictionary = ScheduleStorage.FindSchedulesForPersons(new ScheduleDateTimePeriod(dateTimePeriod), scenario, new PersonProvider(new[] {agent}), new ScheduleDictionaryLoadOptions(false, false), new[] {agent});
			var result = Target.Execute(overtimePreference, new NoSchedulingProgress(), scheduleDictionary, new[] { agent }, dateOnly.ToDateOnlyPeriod(), requestedPeriod, new[] {skill}, new[] {skill}, fullPeriod);
			var overtimeActivities = scheduleDictionary[agent].ScheduledDay(dateOnly).PersonAssignment().OvertimeActivities().Where(ot => ot.Period == new DateTimePeriod(2017, 06, 01, 13, 2017, 06, 01, 15));
			overtimeActivities.Count().Should().Be.EqualTo(1);
			result.Models.Should().Not.Be.Empty();
			result.ResourceCalculationPeriods.Count.Should().Be.EqualTo(16);
		}

	}
}
