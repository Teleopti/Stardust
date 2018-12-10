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


namespace Teleopti.Ccc.DomainTest.Scheduling.Overtime
{
	[DomainTest]
	[AllTogglesOn]
	public class AddOvertimeTest : IIsolateSystem
	{
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
		public AddOverTime Target;
		public FakePersonForOvertimeProvider FakePersonForOvertimeProvider;
		public FakeIntervalLengthFetcher FakeIntervalLengthFetcher;
		private IScenario scenario;
		private IActivity phoneActivity;
		private IActivity emailActivity;
		private ISkill skill;
		private IMultiplicatorDefinitionSet multiplicatorDefinitionSet;
		private IShiftCategory shiftCategory;
		private IContract contract;

		public void Isolate(IIsolate isolate)
		{
			isolate.UseTestDouble<GridlockManager>().For<IGridlockManager>();
			isolate.UseTestDouble<FakeUserTimeZone>().For<IUserTimeZone>();
			isolate.UseTestDouble<FakePersonForOvertimeProvider>().For<IPersonForOvertimeProvider>();
		}

		private void setup()
		{
			FakeIntervalLengthFetcher.Has(60);
			scenario = ScenarioRepository.Has("scenario");
			phoneActivity = ActivityRepository.Has("phoneActivity");
			emailActivity = ActivityRepository.Has("emailActivity");
			skill = SkillRepository.Has("skill", phoneActivity);
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

			FakePersonForOvertimeProvider.Fill(new List<SuggestedPersonsModel> { new SuggestedPersonsModel { PersonId = agent.Id.GetValueOrDefault() } });
			SkillDayRepository.Has(skill.CreateSkillDayWithDemand(scenario, dateOnly, 1));
			var ass = new PersonAssignment(agent, scenario, dateOnly).ShiftCategory(shiftCategory).WithLayer(phoneActivity, new TimePeriod(15, 16));
			PersonAssignmentRepository.Has(ass);
			var overtimePreference = new OvertimePreferences
			{
				OvertimeType = multiplicatorDefinitionSet,
				ScheduleTag = new NullScheduleTag(),
				SelectedSpecificTimePeriod = new TimePeriod(16, 0, 17, 0),
				SelectedTimePeriod = new TimePeriod(1, 0, 1, 0),
				SkillActivity = phoneActivity
			};

			var model = new OverTimeSuggestionModel
			{
				SkillIds = new[] { skill.Id.GetValueOrDefault() },
				TimeSerie = new[] { new DateTime(2017, 06, 1, 0, 0, 0), new DateTime(2017, 06, 2, 0, 0, 0) },
				OvertimePreferences = overtimePreference
			};

			var result = Target.GetSuggestion(model);

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
			var ass = new PersonAssignment(agent, scenario, dateOnly).ShiftCategory(shiftCategory).WithLayer(phoneActivity, new TimePeriod(15, 16));
			PersonAssignmentRepository.Has(ass);
			PersonRepository.Has(agent);
			var agent2 = new Person().WithId().InTimeZone(TimeZoneInfo.Utc).WithPersonPeriod(contract, skill).WithSchedulePeriodOneWeek(dateOnly);
			var ass2 = new PersonAssignment(agent2, scenario, dateOnly).ShiftCategory(shiftCategory).WithLayer(phoneActivity, new TimePeriod(15, 16));
			PersonAssignmentRepository.Has(ass2);
			PersonRepository.Has(agent2);

			FakePersonForOvertimeProvider.Fill(new List<SuggestedPersonsModel> { new SuggestedPersonsModel { PersonId = agent.Id.GetValueOrDefault() }, new SuggestedPersonsModel { PersonId = agent2.Id.GetValueOrDefault() } });

			SkillDayRepository.Has(skill.CreateSkillDayWithDemand(scenario, dateOnly, 1));
			var overtimePreference = new OvertimePreferences
			{
				OvertimeType = multiplicatorDefinitionSet,
				ScheduleTag = new NullScheduleTag(),
				SelectedSpecificTimePeriod = new TimePeriod(16, 0, 17, 0),
				SelectedTimePeriod = new TimePeriod(TimeSpan.FromMinutes(1), TimeSpan.FromHours(1)),
				SkillActivity = phoneActivity
			};
			var model = new OverTimeSuggestionModel
			{
				SkillIds = new[] { skill.Id.GetValueOrDefault() },
				TimeSerie = new[] { new DateTime(2017, 06, 1, 0, 0, 0), new DateTime(2017, 06, 2, 0, 0, 0) },
				OvertimePreferences = overtimePreference
			};

			var result = Target.GetSuggestion(model);

			result.Models.Count.Should().Be.EqualTo(1);
		}

		[Test]
		public void ShouldNotScheduleOvertimeOutSideShiftHours()
		{
			FakeIntervalLengthFetcher.Has(60);
			scenario = ScenarioRepository.Has("scenario");
			phoneActivity = ActivityRepository.Has("phoneActivity");
			emailActivity = ActivityRepository.Has("emailActivity");
			
			var emailSkill = SkillFactory.CreateSkill("email");
			emailSkill.Activity = emailActivity;
			emailSkill.IsOpenBetween(13, 14);
			emailSkill.DefaultResolution = 60;
			emailSkill.SetId(Guid.NewGuid());
			SkillRepository.Add(emailSkill);

			var phoneSkill = SkillFactory.CreateSkill("phone");
			phoneSkill.Activity = phoneActivity;
			phoneSkill.IsOpenBetween(13, 15);
			phoneSkill.DefaultResolution = 30;
			phoneSkill.SetId(Guid.NewGuid());
			SkillRepository.Add(phoneSkill);


			shiftCategory = new ShiftCategory("shiftCategory").WithId();
			multiplicatorDefinitionSet = new MultiplicatorDefinitionSet("overtime", MultiplicatorType.Overtime);
			contract = new Contract("contract");
			contract.AddMultiplicatorDefinitionSetCollection(multiplicatorDefinitionSet);

			Now.Is("2017-06-01 08:00");

			SkillCombinationResourceRepository.PersistSkillCombinationResource(Now.UtcDateTime(), new[]
			{
				new SkillCombinationResource
				{
					StartDateTime = new DateTime(2017, 06, 1, 13, 0, 0).Utc(),
					EndDateTime = new DateTime(2017, 06, 01, 13, 30, 0).Utc(),
					Resource = 8,
					SkillCombination = new[] { phoneSkill.Id.GetValueOrDefault()}
				},
				new SkillCombinationResource
				{
					StartDateTime = new DateTime(2017, 06, 1, 13, 30, 0).Utc(),
					EndDateTime = new DateTime(2017, 06, 01, 14, 0, 0).Utc(),
					Resource = 6,
					SkillCombination = new[] { phoneSkill.Id.GetValueOrDefault()}
				},
				new SkillCombinationResource
				{
					StartDateTime = new DateTime(2017, 06, 1, 14, 0, 0).Utc(),
					EndDateTime = new DateTime(2017, 06, 01, 14, 30, 0).Utc(),
					Resource = 8,
					SkillCombination = new[] { phoneSkill.Id.GetValueOrDefault()}
				},
				new SkillCombinationResource
				{
					StartDateTime = new DateTime(2017, 06, 1, 14, 30, 0).Utc(),
					EndDateTime = new DateTime(2017, 06, 01, 15, 0, 0).Utc(),
					Resource =5,
					SkillCombination = new[] { phoneSkill.Id.GetValueOrDefault()}
				},
				new SkillCombinationResource
				{
					StartDateTime = new DateTime(2017, 06, 1, 13, 0, 0).Utc(),
					EndDateTime = new DateTime(2017, 06, 01, 14, 0, 0).Utc(),
					Resource = 0.1,
					SkillCombination = new[] {emailSkill.Id.GetValueOrDefault()}
				}
			});

			var dateOnly = new DateOnly(2017, 06, 1);

			var agent = new Person().WithId().InTimeZone(TimeZoneInfo.Utc).WithPersonPeriod(contract, phoneSkill, emailSkill).WithSchedulePeriodOneWeek(dateOnly);
			var ass = new PersonAssignment(agent, scenario, dateOnly).ShiftCategory(shiftCategory).WithLayer(phoneActivity, new TimePeriod(12, 13));
			PersonAssignmentRepository.Has(ass);
			PersonRepository.Has(agent);

			var agent2 = new Person().WithId().InTimeZone(TimeZoneInfo.Utc).WithPersonPeriod(contract, phoneSkill, emailSkill).WithSchedulePeriodOneWeek(dateOnly);
			var ass2 = new PersonAssignment(agent2, scenario, dateOnly).ShiftCategory(shiftCategory).WithLayer(phoneActivity, new TimePeriod(12, 13));
			PersonAssignmentRepository.Has(ass2);
			PersonRepository.Has(agent2);

			FakePersonForOvertimeProvider.Fill(new List<SuggestedPersonsModel> {  new SuggestedPersonsModel { PersonId = agent.Id.GetValueOrDefault() }, new SuggestedPersonsModel { PersonId = agent2.Id.GetValueOrDefault() } });

			SkillDayRepository.Has(phoneSkill.CreateSkillDayWithDemand(scenario, dateOnly, 0.1));
			SkillDayRepository.Has(emailSkill.CreateSkillDayWithDemand(scenario, dateOnly, 5));
			var overtimePreference = new OvertimePreferences
			{
				OvertimeType = multiplicatorDefinitionSet,
				ScheduleTag = new NullScheduleTag(),
				SelectedSpecificTimePeriod = new TimePeriod(11, 0, 17, 0),
				SelectedTimePeriod = new TimePeriod(TimeSpan.FromMinutes(1), TimeSpan.FromHours(2)),
				SkillActivity = phoneActivity
			};
			var model = new OverTimeSuggestionModel
			{
				SkillIds = new[] { phoneSkill.Id.GetValueOrDefault(), emailSkill.Id.GetValueOrDefault() },
				TimeSerie = new[] { new DateTime(2017, 06, 1, 0, 0, 0), new DateTime(2017, 06, 2, 0, 0, 0) },
				OvertimePreferences = overtimePreference
			};

			var result = Target.GetSuggestion(model);

			result.Models.Count(x => x.StartDateTime >= new DateTime(2017, 06, 1, 13, 0, 0) && x.EndDateTime <= new DateTime(2017, 06, 1, 14, 0, 0)).Should().Be.EqualTo(2);
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
			PersonRepository.Has(agent);
			var ass = new PersonAssignment(agent, scenario, dateOnly).ShiftCategory(shiftCategory).WithLayer(phoneActivity, new TimePeriod(15, 16));
			PersonAssignmentRepository.Has(ass);

			var agent2 = new Person().WithId().InTimeZone(TimeZoneInfo.Utc).WithPersonPeriod(contract, skill).WithSchedulePeriodOneWeek(dateOnly);
			var ass2 = new PersonAssignment(agent2, scenario, dateOnly).ShiftCategory(shiftCategory).WithLayer(phoneActivity, new TimePeriod(15, 17));
			PersonAssignmentRepository.Has(ass2);

			PersonRepository.Has(agent2);
			FakePersonForOvertimeProvider.Fill(new List<SuggestedPersonsModel> { new SuggestedPersonsModel { PersonId = agent.Id.GetValueOrDefault() }, new SuggestedPersonsModel { PersonId = agent2.Id.GetValueOrDefault() } });

			SkillDayRepository.Has(skill.CreateSkillDayWithDemand(scenario, dateOnly, 2));
			var overtimePreference = new OvertimePreferences
			{
				OvertimeType = multiplicatorDefinitionSet,
				ScheduleTag = new NullScheduleTag(),
				SelectedSpecificTimePeriod = new TimePeriod(16, 0, 19, 0),
				SelectedTimePeriod = new TimePeriod(1, 0, 3, 0),
				SkillActivity = phoneActivity
			};
			var model = new OverTimeSuggestionModel
			{
				SkillIds = new[] { skill.Id.GetValueOrDefault() },
				TimeSerie = new[] { new DateTime(2017, 06, 1, 0, 0, 0), new DateTime(2017, 06, 2, 0, 0, 0) },
				OvertimePreferences = overtimePreference
			};
			var result = Target.GetSuggestion(model);

			result.Models.Count.Should().Be.EqualTo(2);
			result.Models.Count(x => x.StartDateTime == new DateTime(2017, 06, 01, 16, 0, 0)).Should().Be.EqualTo(1);
			result.Models.Count(x => x.StartDateTime == new DateTime(2017, 06, 01, 17, 0, 0)).Should().Be.EqualTo(1);
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
			var ass = new PersonAssignment(agent, scenario, dateOnly).ShiftCategory(shiftCategory).WithLayer(phoneActivity, new TimePeriod(15, 16));
			PersonAssignmentRepository.Has(ass);

			var agent2 = new Person().WithId().InTimeZone(TimeZoneInfo.Utc).WithPersonPeriod(contract, skill).WithSchedulePeriodOneWeek(dateOnly);
			var ass2 = new PersonAssignment(agent2, scenario, dateOnly).ShiftCategory(shiftCategory).WithLayer(phoneActivity, new TimePeriod(15, 16));
			PersonAssignmentRepository.Has(ass2);

			PersonRepository.Has(agent);
			PersonRepository.Has(agent2);
			FakePersonForOvertimeProvider.Fill(new List<SuggestedPersonsModel> { new SuggestedPersonsModel { PersonId = agent.Id.GetValueOrDefault() }, new SuggestedPersonsModel { PersonId = agent2.Id.GetValueOrDefault() } });

			SkillDayRepository.Has(skill.CreateSkillDayWithDemand(scenario, dateOnly, 2));
			var overtimePreference = new OvertimePreferences
			{
				OvertimeType = multiplicatorDefinitionSet,
				ScheduleTag = new NullScheduleTag(),
				SelectedSpecificTimePeriod = new TimePeriod(16, 0, 20, 0),
				SelectedTimePeriod = new TimePeriod(1, 0, 4, 0),
				SkillActivity = phoneActivity
			};

			var model = new OverTimeSuggestionModel
			{
				SkillIds = new[] { skill.Id.GetValueOrDefault() },
				TimeSerie = new[] { new DateTime(2017, 06, 1, 0, 0, 0), new DateTime(2017, 06, 2, 0, 0, 0) },
				OvertimePreferences = overtimePreference
			};
			var result = Target.GetSuggestion(model);

			result.Models.Count.Should().Be.EqualTo(1);
			result.Models.Count(x => x.StartDateTime == new DateTime(2017, 06, 01, 16, 0, 0)).Should().Be.EqualTo(1);
		}

		[Test]
		public void ShouldScheduleOvertimeOnDifferentTimezones()
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

			var ass = new PersonAssignment(agent, scenario, june1DateOnly).ShiftCategory(shiftCategory).WithLayer(phoneActivity, timePeriodInUtc);
			PersonAssignmentRepository.Has(ass);

			// Mountain Standard Time -7H
			var agent2 = new Person().WithId().InTimeZone(TimeZoneInfo.FindSystemTimeZoneById("Mountain Standard Time")).WithPersonPeriod(contract, skill).WithSchedulePeriodOneWeek(june1DateOnly);
			var ass2 = new PersonAssignment(agent2, scenario, may31DateOnly).ShiftCategory(shiftCategory).WithLayer(phoneActivity, timePeriodInUtc);
			PersonAssignmentRepository.Has(ass2);

			PersonRepository.Has(agent);
			PersonRepository.Has(agent2);
			FakePersonForOvertimeProvider.Fill(new List<SuggestedPersonsModel> { new SuggestedPersonsModel { PersonId = agent.Id.GetValueOrDefault() }, new SuggestedPersonsModel { PersonId = agent2.Id.GetValueOrDefault() } });

			SkillDayRepository.Has(skill.CreateSkillDayWithDemand(scenario, may31DateOnly, 2));
			SkillDayRepository.Has(skill.CreateSkillDayWithDemand(scenario, june1DateOnly, 2));

			var overtimePreference = new OvertimePreferences
			{
				OvertimeType = multiplicatorDefinitionSet,
				ScheduleTag = new NullScheduleTag(),
				SelectedSpecificTimePeriod = new TimePeriod(6, 0, 10, 0),
				SelectedTimePeriod = new TimePeriod(1, 0, 4, 0),
				SkillActivity = phoneActivity
			};

			var model = new OverTimeSuggestionModel
			{
				SkillIds = new[] { skill.Id.GetValueOrDefault() },
				TimeSerie = new[] { new DateTime(2017, 06, 1, 0, 0, 0), new DateTime(2017, 06, 2, 0, 0, 0) },
				OvertimePreferences = overtimePreference
			};

			var result = Target.GetSuggestion(model);
			result.Models.Count.Should().Be.EqualTo(2);
			result.Models.Count(x => x.StartDateTime == new DateTime(2017, 06, 01, 6, 0, 0)).Should().Be.EqualTo(2);
			result.Models.Count(x => x.EndDateTime == new DateTime(2017, 06, 01, 10, 0, 0)).Should().Be.EqualTo(2);
		}

		[Test]
		public void ShouldScheduleOvertimeBeforeShift()
		{
			setup();
			Now.Is("2017-05-31 08:00");
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

			var ass = new PersonAssignment(agent, scenario, dateOnly).ShiftCategory(shiftCategory).WithLayer(phoneActivity, timePeriodInUtc);
			PersonAssignmentRepository.Has(ass);

			PersonRepository.Has(agent);

			FakePersonForOvertimeProvider.Fill(new List<SuggestedPersonsModel> { new SuggestedPersonsModel { PersonId = agent.Id.GetValueOrDefault() } });

			SkillDayRepository.Has(skill.CreateSkillDayWithDemand(scenario, dateOnly, 2));
			var overtimePreference = new OvertimePreferences
			{
				OvertimeType = multiplicatorDefinitionSet,
				ScheduleTag = new NullScheduleTag(),
				SelectedSpecificTimePeriod = new TimePeriod(1, 0, 7, 0),
				SelectedTimePeriod = new TimePeriod(1, 0, 4, 0),
				SkillActivity = phoneActivity
			};

			var model = new OverTimeSuggestionModel
			{
				SkillIds = new[] { skill.Id.GetValueOrDefault() },
				TimeSerie = new[] { new DateTime(2017, 06, 1, 0, 0, 0), new DateTime(2017, 06, 2, 0, 0, 0) },
				OvertimePreferences = overtimePreference
			};

			var result = Target.GetSuggestion(model);

			result.Models.Count.Should().Be.EqualTo(1);
			result.Models.Count(x => x.StartDateTime == new DateTime(2017, 06, 01, 2, 0, 0)).Should().Be.EqualTo(1);
			result.Models.Count(x => x.EndDateTime == new DateTime(2017, 06, 01, 6, 0, 0)).Should().Be.EqualTo(1);
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
			var timePeriodInUtc = new DateTimePeriod(2017, 06, 01, 5, 2017, 06, 01, 6);

			var ass = new PersonAssignment(agent, scenario, june1DateOnly).ShiftCategory(shiftCategory).WithLayer(phoneActivity, timePeriodInUtc);
			PersonAssignmentRepository.Has(ass);

			// Mountain Standard Time -7H
			var agent2 = new Person().WithId().InTimeZone(TimeZoneInfo.FindSystemTimeZoneById("Mountain Standard Time")).WithPersonPeriod(contract, skill).WithSchedulePeriodOneWeek(june1DateOnly);

			var ass2 = new PersonAssignment(agent2, scenario, may31DateOnly).ShiftCategory(shiftCategory).WithLayer(phoneActivity, timePeriodInUtc);
			PersonAssignmentRepository.Has(ass2);

			PersonRepository.Has(agent);
			PersonRepository.Has(agent2);
			FakePersonForOvertimeProvider.Fill(new List<SuggestedPersonsModel> { new SuggestedPersonsModel { PersonId = agent.Id.GetValueOrDefault() }, new SuggestedPersonsModel { PersonId = agent2.Id.GetValueOrDefault() } });

			SkillDayRepository.Has(skill.CreateSkillDayWithDemand(scenario, june1DateOnly, 2));
			var overtimePreference = new OvertimePreferences
			{
				OvertimeType = multiplicatorDefinitionSet,
				ScheduleTag = new NullScheduleTag(),
				SelectedSpecificTimePeriod = new TimePeriod(1, 0, 5, 0),
				SelectedTimePeriod = new TimePeriod(1, 0, 4, 0),
				SkillActivity = phoneActivity
			};

			var model = new OverTimeSuggestionModel
			{
				SkillIds = new[] { skill.Id.GetValueOrDefault() },
				TimeSerie = new[] { new DateTime(2017, 06, 1, 0, 0, 0), new DateTime(2017, 06, 2, 0, 0, 0) },
				OvertimePreferences = overtimePreference
			};

			var result = Target.GetSuggestion(model);

			result.Models.Count.Should().Be.EqualTo(2);
			result.Models.Count(x => x.StartDateTime == new DateTime(2017, 06, 01, 1, 0, 0)).Should().Be.EqualTo(2);
			result.Models.Count(x => x.EndDateTime == new DateTime(2017, 06, 01, 5, 0, 0)).Should().Be.EqualTo(2);
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
			var ass = new PersonAssignment(agent, scenario, june1DateOnly).ShiftCategory(shiftCategory).WithLayer(phoneActivity, new TimePeriod(23, 24));
			PersonAssignmentRepository.Has(ass);

			PersonRepository.Has(agent);

			FakePersonForOvertimeProvider.Fill(new List<SuggestedPersonsModel> { new SuggestedPersonsModel { PersonId = agent.Id.GetValueOrDefault() } });

			var overtimePreference = new OvertimePreferences
			{
				OvertimeType = multiplicatorDefinitionSet,
				ScheduleTag = new NullScheduleTag(),
				SelectedSpecificTimePeriod = new TimePeriod(15, 0, 22, 0),
				SelectedTimePeriod = new TimePeriod(1, 0, 4, 0),
				SkillActivity = phoneActivity
			};
			var model = new OverTimeSuggestionModel
			{
				SkillIds = new[] { skill.Id.GetValueOrDefault() },
				TimeSerie = new[] { new DateTime(2017, 06, 1, 0, 0, 0), new DateTime(2017, 06, 2, 0, 0, 0) },
				OvertimePreferences = overtimePreference
			};

			var result = Target.GetSuggestion(model);

			result.Models.Should().Not.Be.Empty();
			result.Models.Count(x => x.StartDateTime == new DateTime(2017, 06, 02, 0, 0, 0)).Should().Be.EqualTo(1);
			result.Models.Count(x => x.EndDateTime == new DateTime(2017, 06, 02, 4, 0, 0)).Should().Be.EqualTo(1);
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
			var ass = new PersonAssignment(agent, scenario, june1DateOnly).ShiftCategory(shiftCategory).WithLayer(phoneActivity, new TimePeriod(22, 23));
			PersonAssignmentRepository.Has(ass);

			PersonRepository.Has(agent);
			FakePersonForOvertimeProvider.Fill(new List<SuggestedPersonsModel> { new SuggestedPersonsModel { PersonId = agent.Id.GetValueOrDefault() } });

			var overtimePreference = new OvertimePreferences
			{
				OvertimeType = multiplicatorDefinitionSet,
				ScheduleTag = new NullScheduleTag(),
				SelectedTimePeriod = new TimePeriod(1, 0, 4, 0),
				SelectedSpecificTimePeriod = new TimePeriod(22, 0, 28, 0)
			};

			var model = new OverTimeSuggestionModel
			{
				SkillIds = new[] { skill.Id.GetValueOrDefault() },
				TimeSerie = new[] { new DateTime(2017, 06, 1, 0, 0, 0), new DateTime(2017, 06, 3, 0, 0, 0) },
				OvertimePreferences = overtimePreference
			};

			var result = Target.GetSuggestion(model);

			result.Models.Should().Not.Be.Empty();
			result.Models.Count(x => x.StartDateTime == new DateTime(2017, 06, 01, 23, 0, 0)).Should().Be.EqualTo(1);
			result.Models.Count(x => x.EndDateTime == new DateTime(2017, 06, 02, 3, 0, 0)).Should().Be.EqualTo(1);
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

			PersonRepository.Has(agent);
			FakePersonForOvertimeProvider.Fill(new List<SuggestedPersonsModel> { new SuggestedPersonsModel { PersonId = agent.Id.GetValueOrDefault() } });

			SkillDayRepository.Has(skill.CreateSkillDayWithDemand(scenario, june1DateOnly, 1));
			SkillDayRepository.Has(skill.CreateSkillDayWithDemand(scenario, june2DateOnly, 1));
			var ass = new PersonAssignment(agent, scenario, june1DateOnly).ShiftCategory(shiftCategory).WithLayer(phoneActivity, new TimePeriod(16, 17));
			PersonAssignmentRepository.Has(ass);
			var overtimePreference = new OvertimePreferences
			{
				OvertimeType = multiplicatorDefinitionSet,
				ScheduleTag = new NullScheduleTag(),
				SelectedTimePeriod = new TimePeriod(1, 0, 4, 0),
				SelectedSpecificTimePeriod = new TimePeriod(22, 0, 28, 0)
			};

			var model = new OverTimeSuggestionModel
			{
				SkillIds = new[] { skill.Id.GetValueOrDefault() },
				TimeSerie = new[] { new DateTime(2017, 06, 1, 0, 0, 0), new DateTime(2017, 06, 3, 0, 0, 0) },
				OvertimePreferences = overtimePreference
			};

			var result = Target.GetSuggestion(model);

			result.Models.Count.Should().Be.EqualTo(1);
			result.Models.Count(x => x.StartDateTime == new DateTime(2017, 06, 01, 23, 0, 0)).Should().Be.EqualTo(1);
			result.Models.Count(x => x.EndDateTime == new DateTime(2017, 06, 02, 3, 0, 0)).Should().Be.EqualTo(1);
		}


		[Test]
		public void ShouldScheduleOvertimeOnAgentInDenverOverDenverMidnight()
		{
			setup();
			Now.Is("2017-06-01 08:00");
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

			PersonRepository.Has(agent);
			FakePersonForOvertimeProvider.Fill(new List<SuggestedPersonsModel> { new SuggestedPersonsModel { PersonId = agent.Id.GetValueOrDefault() } });

			SkillDayRepository.Has(skill.CreateSkillDayWithDemand(scenario, june1DateOnly, 1));
			SkillDayRepository.Has(skill.CreateSkillDayWithDemand(scenario, june2DateOnly, 1));
			var ass = new PersonAssignment(agent, scenario, june1DateOnly).ShiftCategory(shiftCategory).WithLayer(phoneActivity, new TimePeriod(22, 23));
			PersonAssignmentRepository.Has(ass);
			var overtimePreference = new OvertimePreferences
			{
				OvertimeType = multiplicatorDefinitionSet,
				ScheduleTag = new NullScheduleTag(),
				SelectedTimePeriod = new TimePeriod(1, 0, 4, 0),
				SelectedSpecificTimePeriod = new TimePeriod(4, 0, 9, 0)
			};

			var model = new OverTimeSuggestionModel
			{
				SkillIds = new[] { skill.Id.GetValueOrDefault() },
				TimeSerie = new[] { new DateTime(2017, 06, 2, 0, 0, 0), new DateTime(2017, 06, 3, 0, 0, 0) },
				OvertimePreferences = overtimePreference
			};

			var result = Target.GetSuggestion(model);

			result.Models.Count.Should().Be.EqualTo(1);
			result.Models.Count(x => x.StartDateTime == new DateTime(2017, 06, 02, 5, 0, 0)).Should().Be.EqualTo(1);
			result.Models.Count(x => x.EndDateTime == new DateTime(2017, 06, 02, 9, 0, 0)).Should().Be.EqualTo(1);
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

			PersonRepository.Has(agent);
			FakePersonForOvertimeProvider.Fill(new List<SuggestedPersonsModel> { new SuggestedPersonsModel { PersonId = agent.Id.GetValueOrDefault() } });

			SkillDayRepository.Has(skill.CreateSkillDayWithDemand(scenario, dateOnly, 1));
			var ass = new PersonAssignment(agent, scenario, dateOnly).ShiftCategory(shiftCategory).WithLayer(phoneActivity, new TimePeriod(15, 16));
			PersonAssignmentRepository.Has(ass);
			var overtimePreference = new OvertimePreferences
			{
				OvertimeType = multiplicatorDefinitionSet,
				ScheduleTag = new NullScheduleTag(),
				SelectedTimePeriod = new TimePeriod(1, 0, 4, 0),
				SelectedSpecificTimePeriod = new TimePeriod(13, 0, 16, 0)
			};
			var model = new OverTimeSuggestionModel
			{
				SkillIds = new[] { skill.Id.GetValueOrDefault() },
				TimeSerie = new[] { new DateTime(2017, 06, 1, 0, 0, 0), new DateTime(2017, 06, 2, 0, 0, 0) },
				OvertimePreferences = overtimePreference
			};

			var result = Target.GetSuggestion(model);

			result.Models.Should().Not.Be.Empty();
			result.ResourceCalculationPeriods.Count.Should().Be.EqualTo(24);
		}

		[Test]
		public void ShouldNotBreakNightlyRestAfterShift()
		{
			setup();
			SkillCombinationResourceRepository.PersistSkillCombinationResource(Now.UtcDateTime(), new[]
			{
				new SkillCombinationResource
				{
					StartDateTime = new DateTime(2017, 06, 1, 19, 0, 0).Utc(),
					EndDateTime = new DateTime(2017, 06, 01, 20, 0, 0).Utc(),
					Resource = 1,
					SkillCombination = new[] {skill.Id.GetValueOrDefault()}
				},
				new SkillCombinationResource
				{
					StartDateTime = new DateTime(2017, 06, 2, 7, 0, 0).Utc(),
					EndDateTime = new DateTime(2017, 06, 2, 8, 0, 0).Utc(),
					Resource = 1,
					SkillCombination = new[] {skill.Id.GetValueOrDefault()}
				}
			});

			var dateOnlyJune1 = new DateOnly(2017, 06, 1);
			var dateOnlyJune2 = new DateOnly(2017, 06, 2);

			var agent = new Person().WithId().InTimeZone(TimeZoneInfo.Utc).WithPersonPeriod(contract, skill).WithSchedulePeriodOneWeek(dateOnlyJune1);
			PersonRepository.Has(agent);

			FakePersonForOvertimeProvider.Fill(new List<SuggestedPersonsModel> { new SuggestedPersonsModel { PersonId = agent.Id.GetValueOrDefault() } });
			SkillDayRepository.Has(skill.CreateSkillDayWithDemand(scenario, dateOnlyJune1, 1));
			SkillDayRepository.Has(skill.CreateSkillDayWithDemand(scenario, dateOnlyJune2, 1));
			var assJune1 = new PersonAssignment(agent, scenario, dateOnlyJune1).ShiftCategory(shiftCategory).WithLayer(phoneActivity, new TimePeriod(19, 20));
			var assJune2 = new PersonAssignment(agent, scenario, dateOnlyJune2).ShiftCategory(shiftCategory).WithLayer(phoneActivity, new TimePeriod(07, 08));
			PersonAssignmentRepository.Has(assJune1);
			PersonAssignmentRepository.Has(assJune2);

			var overtimePreference = new OvertimePreferences
			{
				OvertimeType = multiplicatorDefinitionSet,
				ScheduleTag = new NullScheduleTag(),
				SelectedSpecificTimePeriod = new TimePeriod(20, 0, 24, 0),
				SelectedTimePeriod = new TimePeriod(1, 0, 4, 0),
				SkillActivity = phoneActivity
			};

			var model = new OverTimeSuggestionModel
			{
				SkillIds = new[] { skill.Id.GetValueOrDefault() },
				TimeSerie = new[] { new DateTime(2017, 06, 1, 0, 0, 0), new DateTime(2017, 06, 2, 0, 0, 0) },
				OvertimePreferences = overtimePreference
			};

			var result = Target.GetSuggestion(model);

			result.Models.Should().Be.Empty();
		}

		[Test]
		public void ShouldNotBreakNightlyRestBeforeShift()
		{
			setup();
			SkillCombinationResourceRepository.PersistSkillCombinationResource(Now.UtcDateTime(), new[]
			{
				new SkillCombinationResource
				{
					StartDateTime = new DateTime(2017, 06, 1, 19, 0, 0).Utc(),
					EndDateTime = new DateTime(2017, 06, 01, 20, 0, 0).Utc(),
					Resource = 1,
					SkillCombination = new[] {skill.Id.GetValueOrDefault()}
				},
				new SkillCombinationResource
				{
					StartDateTime = new DateTime(2017, 06, 2, 7, 0, 0).Utc(),
					EndDateTime = new DateTime(2017, 06, 2, 8, 0, 0).Utc(),
					Resource = 1,
					SkillCombination = new[] {skill.Id.GetValueOrDefault()}
				}
			});

			var dateOnlyJune1 = new DateOnly(2017, 06, 1);
			var dateOnlyJune2 = new DateOnly(2017, 06, 2);

			var agent = new Person().WithId().InTimeZone(TimeZoneInfo.Utc).WithPersonPeriod(contract, skill).WithSchedulePeriodOneWeek(dateOnlyJune1);
			PersonRepository.Has(agent);

			FakePersonForOvertimeProvider.Fill(new List<SuggestedPersonsModel> { new SuggestedPersonsModel { PersonId = agent.Id.GetValueOrDefault() } });
			SkillDayRepository.Has(skill.CreateSkillDayWithDemand(scenario, dateOnlyJune1, 1));
			SkillDayRepository.Has(skill.CreateSkillDayWithDemand(scenario, dateOnlyJune2, 1));
			var assJune1 = new PersonAssignment(agent, scenario, dateOnlyJune1).ShiftCategory(shiftCategory).WithLayer(phoneActivity, new TimePeriod(19, 20));
			var assJune2 = new PersonAssignment(agent, scenario, dateOnlyJune2).ShiftCategory(shiftCategory).WithLayer(phoneActivity, new TimePeriod(07, 08));
			PersonAssignmentRepository.Has(assJune1);
			PersonAssignmentRepository.Has(assJune2);

			var overtimePreference = new OvertimePreferences
			{
				OvertimeType = multiplicatorDefinitionSet,
				ScheduleTag = new NullScheduleTag(),
				SelectedSpecificTimePeriod = new TimePeriod(3, 0, 7, 0),
				SelectedTimePeriod = new TimePeriod(1, 0, 4, 0),
				SkillActivity = phoneActivity
			};

			var model = new OverTimeSuggestionModel
			{
				SkillIds = new[] { skill.Id.GetValueOrDefault() },
				TimeSerie = new[] { new DateTime(2017, 06, 2, 0, 0, 0), new DateTime(2017, 06, 3, 0, 0, 0) },
				OvertimePreferences = overtimePreference
			};

			var result = Target.GetSuggestion(model);

			result.Models.Should().Be.Empty();
		}

		[Test]
		public void ShouldNotBreakWeeklyRestAfterShift()
		{
			setup();
			SkillCombinationResourceRepository.PersistSkillCombinationResource(Now.UtcDateTime(), new[]
			{
				new SkillCombinationResource
				{
					StartDateTime = new DateTime(2017, 06, 5, 16, 0, 0).Utc(),
					EndDateTime = new DateTime(2017, 06, 5, 17, 0, 0).Utc(),
					Resource = 1,
					SkillCombination = new[] {skill.Id.GetValueOrDefault()}
				}
			});
			
			var dateOnlyJune5 = new DateOnly(2017, 06, 5);
			var dateOnlyJune6 = new DateOnly(2017, 06, 6);
			var dateOnlyJune7 = new DateOnly(2017, 06, 7);
			var dateOnlyJune8 = new DateOnly(2017, 06, 8);
			var dateOnlyJune9 = new DateOnly(2017, 06, 9);
			var dateOnlyJune10 = new DateOnly(2017, 06, 10);
			var dateOnlyJune11 = new DateOnly(2017, 06, 11);

			var agent = new Person().WithId().InTimeZone(TimeZoneInfo.Utc).WithPersonPeriod(contract, skill).WithSchedulePeriodOneWeek(dateOnlyJune5);
			PersonRepository.Has(agent);

			FakePersonForOvertimeProvider.Fill(new List<SuggestedPersonsModel> { new SuggestedPersonsModel { PersonId = agent.Id.GetValueOrDefault() } });
			
			SkillDayRepository.Has(skill.CreateSkillDayWithDemand(scenario, dateOnlyJune5, 1));
			SkillDayRepository.Has(skill.CreateSkillDayWithDemand(scenario, dateOnlyJune6, 1));
			SkillDayRepository.Has(skill.CreateSkillDayWithDemand(scenario, dateOnlyJune7, 1));
			SkillDayRepository.Has(skill.CreateSkillDayWithDemand(scenario, dateOnlyJune8, 1));
			SkillDayRepository.Has(skill.CreateSkillDayWithDemand(scenario, dateOnlyJune9, 1));
			SkillDayRepository.Has(skill.CreateSkillDayWithDemand(scenario, dateOnlyJune10, 1));
			SkillDayRepository.Has(skill.CreateSkillDayWithDemand(scenario, dateOnlyJune11, 1));

			PersonAssignmentRepository.Has(new PersonAssignment(agent, scenario, dateOnlyJune5).ShiftCategory(shiftCategory).WithLayer(phoneActivity, new TimePeriod(16, 17)));
			//36 hours between
			PersonAssignmentRepository.Has(new PersonAssignment(agent, scenario, dateOnlyJune7).ShiftCategory(shiftCategory).WithLayer(phoneActivity, new TimePeriod(5, 17)));
			PersonAssignmentRepository.Has(new PersonAssignment(agent, scenario, dateOnlyJune8).ShiftCategory(shiftCategory).WithLayer(phoneActivity, new TimePeriod(8, 17)));
			PersonAssignmentRepository.Has(new PersonAssignment(agent, scenario, dateOnlyJune9).ShiftCategory(shiftCategory).WithLayer(phoneActivity, new TimePeriod(8, 17)));
			PersonAssignmentRepository.Has(new PersonAssignment(agent, scenario, dateOnlyJune10).ShiftCategory(shiftCategory).WithLayer(phoneActivity, new TimePeriod(8, 17)));
			PersonAssignmentRepository.Has(new PersonAssignment(agent, scenario, dateOnlyJune11).ShiftCategory(shiftCategory).WithLayer(phoneActivity, new TimePeriod(8, 17)));

			var overtimePreference = new OvertimePreferences
			{
				OvertimeType = multiplicatorDefinitionSet,
				ScheduleTag = new NullScheduleTag(),
				SelectedSpecificTimePeriod = new TimePeriod(17, 0, 21, 0),
				SelectedTimePeriod = new TimePeriod(1, 0, 4, 0),
				SkillActivity = phoneActivity
			};

			var model = new OverTimeSuggestionModel
			{
				SkillIds = new[] { skill.Id.GetValueOrDefault() },
				TimeSerie = new[] { new DateTime(2017, 06, 5, 0, 0, 0), new DateTime(2017, 06, 6, 0, 0, 0) },
				OvertimePreferences = overtimePreference
			};

			var result = Target.GetSuggestion(model);

			result.Models.Should().Be.Empty();
		}

		[Test]
		public void ShouldNotBreakWeeklyRestBeforeShift()
		{
			setup();
			SkillCombinationResourceRepository.PersistSkillCombinationResource(Now.UtcDateTime(), new[]
			{
				new SkillCombinationResource
				{
					StartDateTime = new DateTime(2017, 06, 11, 8, 0, 0).Utc(),
					EndDateTime = new DateTime(2017, 06, 11, 9, 0, 0).Utc(),
					Resource = 1,
					SkillCombination = new[] {skill.Id.GetValueOrDefault()}
				}
			});

			var dateOnlyJune5 = new DateOnly(2017, 06, 5);
			var dateOnlyJune6 = new DateOnly(2017, 06, 6);
			var dateOnlyJune7 = new DateOnly(2017, 06, 7);
			var dateOnlyJune8 = new DateOnly(2017, 06, 8);
			var dateOnlyJune9 = new DateOnly(2017, 06, 9);
			var dateOnlyJune10 = new DateOnly(2017, 06, 10);
			var dateOnlyJune11 = new DateOnly(2017, 06, 11);

			var agent = new Person().WithId().InTimeZone(TimeZoneInfo.Utc).WithPersonPeriod(contract, skill).WithSchedulePeriodOneWeek(dateOnlyJune5);
			PersonRepository.Has(agent);

			FakePersonForOvertimeProvider.Fill(new List<SuggestedPersonsModel> { new SuggestedPersonsModel { PersonId = agent.Id.GetValueOrDefault() } });

			SkillDayRepository.Has(skill.CreateSkillDayWithDemand(scenario, dateOnlyJune5, 1));
			SkillDayRepository.Has(skill.CreateSkillDayWithDemand(scenario, dateOnlyJune6, 1));
			SkillDayRepository.Has(skill.CreateSkillDayWithDemand(scenario, dateOnlyJune7, 1));
			SkillDayRepository.Has(skill.CreateSkillDayWithDemand(scenario, dateOnlyJune8, 1));
			SkillDayRepository.Has(skill.CreateSkillDayWithDemand(scenario, dateOnlyJune9, 1));
			SkillDayRepository.Has(skill.CreateSkillDayWithDemand(scenario, dateOnlyJune10, 1));
			SkillDayRepository.Has(skill.CreateSkillDayWithDemand(scenario, dateOnlyJune11, 1));

			PersonAssignmentRepository.Has(new PersonAssignment(agent, scenario, dateOnlyJune5).ShiftCategory(shiftCategory).WithLayer(phoneActivity, new TimePeriod(8, 17)));
			PersonAssignmentRepository.Has(new PersonAssignment(agent, scenario, dateOnlyJune6).ShiftCategory(shiftCategory).WithLayer(phoneActivity, new TimePeriod(8, 17)));
			PersonAssignmentRepository.Has(new PersonAssignment(agent, scenario, dateOnlyJune7).ShiftCategory(shiftCategory).WithLayer(phoneActivity, new TimePeriod(8, 17)));
			PersonAssignmentRepository.Has(new PersonAssignment(agent, scenario, dateOnlyJune8).ShiftCategory(shiftCategory).WithLayer(phoneActivity, new TimePeriod(8, 17)));
			PersonAssignmentRepository.Has(new PersonAssignment(agent, scenario, dateOnlyJune9).ShiftCategory(shiftCategory).WithLayer(phoneActivity, new TimePeriod(8, 20)));
			//36 hours between
			PersonAssignmentRepository.Has(new PersonAssignment(agent, scenario, dateOnlyJune11).ShiftCategory(shiftCategory).WithLayer(phoneActivity, new TimePeriod(8, 9)));

			var overtimePreference = new OvertimePreferences
			{
				OvertimeType = multiplicatorDefinitionSet,
				ScheduleTag = new NullScheduleTag(),
				SelectedSpecificTimePeriod = new TimePeriod(4, 0, 8, 0),
				SelectedTimePeriod = new TimePeriod(1, 0, 4, 0),
				SkillActivity = phoneActivity
			};

			var model = new OverTimeSuggestionModel
			{
				SkillIds = new[] { skill.Id.GetValueOrDefault() },
				TimeSerie = new[] { new DateTime(2017, 06, 11, 0, 0, 0), new DateTime(2017, 06, 12, 0, 0, 0) },
				OvertimePreferences = overtimePreference
			};

			var result = Target.GetSuggestion(model);

			result.Models.Should().Be.Empty();
		}

		[Test]
		public void ShouldNotBreakMaxWeeklyWorkTimeBeforeShift()
		{
			setup();
			SkillCombinationResourceRepository.PersistSkillCombinationResource(Now.UtcDateTime(), new[]
			{
				new SkillCombinationResource
				{
					StartDateTime = new DateTime(2017, 06, 11, 9, 0, 0).Utc(),
					EndDateTime = new DateTime(2017, 06, 11, 10, 0, 0).Utc(),
					Resource = 1,
					SkillCombination = new[] {skill.Id.GetValueOrDefault()}
				}
			});

			var dateOnlyJune5 = new DateOnly(2017, 06, 5);
			var dateOnlyJune6 = new DateOnly(2017, 06, 6);
			var dateOnlyJune7 = new DateOnly(2017, 06, 7);
			var dateOnlyJune8 = new DateOnly(2017, 06, 8);
			var dateOnlyJune9 = new DateOnly(2017, 06, 9);
			var dateOnlyJune10 = new DateOnly(2017, 06, 10);
			var dateOnlyJune11 = new DateOnly(2017, 06, 11);

			var agent = new Person().WithId().InTimeZone(TimeZoneInfo.Utc).WithPersonPeriod(contract, skill).WithSchedulePeriodOneWeek(dateOnlyJune5);
			PersonRepository.Has(agent);

			FakePersonForOvertimeProvider.Fill(new List<SuggestedPersonsModel> { new SuggestedPersonsModel { PersonId = agent.Id.GetValueOrDefault() } });

			SkillDayRepository.Has(skill.CreateSkillDayWithDemand(scenario, dateOnlyJune5, 1));
			SkillDayRepository.Has(skill.CreateSkillDayWithDemand(scenario, dateOnlyJune6, 1));
			SkillDayRepository.Has(skill.CreateSkillDayWithDemand(scenario, dateOnlyJune7, 1));
			SkillDayRepository.Has(skill.CreateSkillDayWithDemand(scenario, dateOnlyJune8, 1));
			SkillDayRepository.Has(skill.CreateSkillDayWithDemand(scenario, dateOnlyJune9, 1));
			SkillDayRepository.Has(skill.CreateSkillDayWithDemand(scenario, dateOnlyJune10, 1));
			SkillDayRepository.Has(skill.CreateSkillDayWithDemand(scenario, dateOnlyJune11, 1));

			PersonAssignmentRepository.Has(new PersonAssignment(agent, scenario, dateOnlyJune5).ShiftCategory(shiftCategory).WithLayer(phoneActivity, new TimePeriod(9, 17)));
			PersonAssignmentRepository.Has(new PersonAssignment(agent, scenario, dateOnlyJune6).ShiftCategory(shiftCategory).WithLayer(phoneActivity, new TimePeriod(9, 17)));
			PersonAssignmentRepository.Has(new PersonAssignment(agent, scenario, dateOnlyJune7).ShiftCategory(shiftCategory).WithLayer(phoneActivity, new TimePeriod(9, 17)));
			PersonAssignmentRepository.Has(new PersonAssignment(agent, scenario, dateOnlyJune8).ShiftCategory(shiftCategory).WithLayer(phoneActivity, new TimePeriod(9, 17)));
			PersonAssignmentRepository.Has(new PersonAssignment(agent, scenario, dateOnlyJune9).ShiftCategory(shiftCategory).WithLayer(phoneActivity, new TimePeriod(9, 17)));
			PersonAssignmentRepository.Has(new PersonAssignment(agent, scenario, dateOnlyJune11).ShiftCategory(shiftCategory).WithLayer(phoneActivity, new TimePeriod(9, 17)));

			var overtimePreference = new OvertimePreferences
			{
				OvertimeType = multiplicatorDefinitionSet,
				ScheduleTag = new NullScheduleTag(),
				SelectedSpecificTimePeriod = new TimePeriod(5, 0, 9, 0),
				SelectedTimePeriod = new TimePeriod(1, 0, 4, 0),
				SkillActivity = phoneActivity
			};

			var model = new OverTimeSuggestionModel
			{
				SkillIds = new[] { skill.Id.GetValueOrDefault() },
				TimeSerie = new[] { new DateTime(2017, 06, 11, 0, 0, 0), new DateTime(2017, 06, 12, 0, 0, 0) },
				OvertimePreferences = overtimePreference
			};

			var result = Target.GetSuggestion(model);

			result.Models.Should().Be.Empty();
		}

		[Test]
		public void ShouldNotBreakMaxWeeklyWorkTimeAfterShift()
		{
			setup();
			SkillCombinationResourceRepository.PersistSkillCombinationResource(Now.UtcDateTime(), new[]
			{
				new SkillCombinationResource
				{
					StartDateTime = new DateTime(2017, 06, 5, 16, 0, 0).Utc(),
					EndDateTime = new DateTime(2017, 06, 5, 17, 0, 0).Utc(),
					Resource = 1,
					SkillCombination = new[] {skill.Id.GetValueOrDefault()}
				}
			});

			var dateOnlyJune5 = new DateOnly(2017, 06, 5);
			var dateOnlyJune6 = new DateOnly(2017, 06, 6);
			var dateOnlyJune7 = new DateOnly(2017, 06, 7);
			var dateOnlyJune8 = new DateOnly(2017, 06, 8);
			var dateOnlyJune9 = new DateOnly(2017, 06, 9);
			var dateOnlyJune10 = new DateOnly(2017, 06, 10);
			var dateOnlyJune11 = new DateOnly(2017, 06, 11);

			var agent = new Person().WithId().InTimeZone(TimeZoneInfo.Utc).WithPersonPeriod(contract, skill).WithSchedulePeriodOneWeek(dateOnlyJune5);
			PersonRepository.Has(agent);

			FakePersonForOvertimeProvider.Fill(new List<SuggestedPersonsModel> { new SuggestedPersonsModel { PersonId = agent.Id.GetValueOrDefault() } });

			SkillDayRepository.Has(skill.CreateSkillDayWithDemand(scenario, dateOnlyJune5, 1));
			SkillDayRepository.Has(skill.CreateSkillDayWithDemand(scenario, dateOnlyJune6, 1));
			SkillDayRepository.Has(skill.CreateSkillDayWithDemand(scenario, dateOnlyJune7, 1));
			SkillDayRepository.Has(skill.CreateSkillDayWithDemand(scenario, dateOnlyJune8, 1));
			SkillDayRepository.Has(skill.CreateSkillDayWithDemand(scenario, dateOnlyJune9, 1));
			SkillDayRepository.Has(skill.CreateSkillDayWithDemand(scenario, dateOnlyJune10, 1));
			SkillDayRepository.Has(skill.CreateSkillDayWithDemand(scenario, dateOnlyJune11, 1));

			PersonAssignmentRepository.Has(new PersonAssignment(agent, scenario, dateOnlyJune5).ShiftCategory(shiftCategory).WithLayer(phoneActivity, new TimePeriod(9, 17)));
			PersonAssignmentRepository.Has(new PersonAssignment(agent, scenario, dateOnlyJune7).ShiftCategory(shiftCategory).WithLayer(phoneActivity, new TimePeriod(9, 17)));
			PersonAssignmentRepository.Has(new PersonAssignment(agent, scenario, dateOnlyJune8).ShiftCategory(shiftCategory).WithLayer(phoneActivity, new TimePeriod(9, 17)));
			PersonAssignmentRepository.Has(new PersonAssignment(agent, scenario, dateOnlyJune9).ShiftCategory(shiftCategory).WithLayer(phoneActivity, new TimePeriod(9, 17)));
			PersonAssignmentRepository.Has(new PersonAssignment(agent, scenario, dateOnlyJune10).ShiftCategory(shiftCategory).WithLayer(phoneActivity, new TimePeriod(9, 17)));
			PersonAssignmentRepository.Has(new PersonAssignment(agent, scenario, dateOnlyJune11).ShiftCategory(shiftCategory).WithLayer(phoneActivity, new TimePeriod(9, 17)));

			var overtimePreference = new OvertimePreferences
			{
				OvertimeType = multiplicatorDefinitionSet,
				ScheduleTag = new NullScheduleTag(),
				SelectedSpecificTimePeriod = new TimePeriod(17, 0, 21, 0),
				SelectedTimePeriod = new TimePeriod(1, 0, 4, 0),
				SkillActivity = phoneActivity
			};

			var model = new OverTimeSuggestionModel
			{
				SkillIds = new[] { skill.Id.GetValueOrDefault() },
				TimeSerie = new[] { new DateTime(2017, 06, 5, 0, 0, 0), new DateTime(2017, 06, 6, 0, 0, 0) },
				OvertimePreferences = overtimePreference
			};

			var result = Target.GetSuggestion(model);

			result.Models.Should().Be.Empty();
		}

		[Test]
		public void ShouldVerifyIfTheObjectsAreSame()
		{
			var skill1 = Guid.NewGuid();
			var skill2 = Guid.NewGuid();
			var scr1 = new SkillCombinationResource()
			{
				StartDateTime = new DateTime(2017, 08, 15, 8, 0, 0),
				EndDateTime = new DateTime(2017, 08, 15, 8, 0, 0),
				Resource = 10,
				SkillCombination = new [] {skill1, skill2}
			};

			var scr2 = new SkillCombinationResource()
			{
				StartDateTime = new DateTime(2017, 08, 15, 8, 0, 0),
				EndDateTime = new DateTime(2017, 08, 15, 8, 0, 0),
				Resource = 10,
				SkillCombination = new[] {  skill2, skill1 }
			};

			Assert.True(scr1.Equals(scr2));
		}

	}
}
