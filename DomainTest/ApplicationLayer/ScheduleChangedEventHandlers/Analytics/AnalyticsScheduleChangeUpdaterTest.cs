using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Analytics;
using Teleopti.Ccc.Domain.ApplicationLayer.Events;
using Teleopti.Ccc.Domain.ApplicationLayer.ScheduleChangedEventHandlers;
using Teleopti.Ccc.Domain.ApplicationLayer.ScheduleChangedEventHandlers.Analytics;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.Exceptions;
using Teleopti.Ccc.Domain.FeatureFlags;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure.Analytics;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.IocCommon;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.TestCommon.FakeRepositories;
using Teleopti.Ccc.TestCommon.IoC;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.ApplicationLayer.ScheduleChangedEventHandlers.Analytics
{
	[TestFixture]
	[DomainTest]
	public class AnalyticsScheduleChangeUpdaterTest : ISetup
	{
		private const int intervalLengthInMinute = 15;
		public AnalyticsScheduleChangeUpdater Target;
		public FakeAnalyticsScheduleRepository AnalyticsSchedules;
		public FakeAnalyticsShiftCategoryRepository AnalyticsShiftCategories;
		public FakeAnalyticsScenarioRepository AnalyticsScenarios;
		public FakeAnalyticsPersonPeriodRepository AnalyticsPersonPeriods;
		public FakeAnalyticsDateRepository AnalyticsDates;
		public FakeAnalyticsAbsenceRepository AnalyticsAbsences;
		public IScheduleStorage Schedules;
		public IBusinessUnitRepository BusinessUnits;
		public IScenarioRepository Scenarios;
		public IPersonRepository Persons;
		public FakeIntervalLengthFetcher IntervalLength;
		public FakeAnalyticsDayOffRepository AnalyticsDayOffRepository;

		private readonly Guid businessUnitId = Guid.NewGuid();
		private readonly Guid scenarioId = Guid.NewGuid();
		private readonly Guid personId = Guid.NewGuid();
		private readonly Guid personPeriodId = Guid.NewGuid();
		private readonly Guid absenceId = Guid.NewGuid();
		private readonly DateTime startDate = new DateTime(2017, 03, 07, 8, 0, 0, DateTimeKind.Utc);
		private readonly DateTime endDate = new DateTime(2017, 03, 07, 17, 0, 0, DateTimeKind.Utc);

		public void Setup(ISystem system, IIocConfiguration configuration)
		{
			system.UseTestDouble<FakeScheduleStorage_DoNotUse>().For<IScheduleStorage>();
			system.AddService<AnalyticsScheduleChangeUpdater>();
		}

		[Test]
		public void ShouldNotTryToSaveScheduleChangeWhenDateIdMappingFails()
		{
			AnalyticsDates.HasDatesBetween(new DateTime(1999, 12, 31), new DateTime(2030, 12, 31));
			var businessUnit = BusinessUnitFactory.CreateSimpleBusinessUnit("Default BU").WithId(businessUnitId);
			var scenario = ScenarioFactory.CreateScenario("Default", true, true).WithId(scenarioId);
			var person = PersonFactory.CreatePerson().WithId(personId);
			var personPeriod = PersonPeriodFactory.CreatePersonPeriod(new DateOnly(startDate)).WithId(personPeriodId);
			personPeriod.Team = TeamFactory.CreateTeam("Team", "Site");
			person.AddPersonPeriod(personPeriod);
			var assignment = PersonAssignmentFactory.CreateEmptyAssignment(person, scenario,
				new DateTimePeriod(startDate, endDate));

			var analyticsScenario = new AnalyticsScenario
			{
				ScenarioCode = scenarioId,
				ScenarioId = 3
			};
			var scheduleDay = new ProjectionChangedEventScheduleDay
			{
				Date = startDate.Date,
				PersonPeriodId = personPeriodId
			};

			var analyticsPersonPeriod = new AnalyticsPersonPeriod
			{
				PersonId = 1,
				BusinessUnitId = 2,
				PersonPeriodCode = scheduleDay.PersonPeriodId
			};
			var existingSchedule = new FactScheduleRow
			{
				DatePart = new AnalyticsFactScheduleDate
				{
					ScheduleDateId = AnalyticsDates.Date(scheduleDay.Date).DateId
				},
				PersonPart = new AnalyticsFactSchedulePerson
				{
					BusinessUnitId = analyticsPersonPeriod.BusinessUnitId,
					PersonId = analyticsPersonPeriod.PersonId
				},
				TimePart = new AnalyticsFactScheduleTime
				{
					ScenarioId = analyticsScenario.ScenarioId
				}
			};

			BusinessUnits.Add(businessUnit);
			Scenarios.Add(scenario);
			Persons.Add(person);
			Schedules.Add(assignment);

			AnalyticsScenarios.AddScenario(analyticsScenario);
			AnalyticsPersonPeriods.AddOrUpdatePersonPeriod(analyticsPersonPeriod);
			AnalyticsSchedules.PersistFactScheduleBatch(new List<IFactScheduleRow> {existingSchedule});
			AnalyticsDates.Clear();

			Target.Handle(new ProjectionChangedEvent
			{
				ScheduleDays = new Collection<ProjectionChangedEventScheduleDay> {scheduleDay},
				ScenarioId = scenarioId,
				LogOnBusinessUnitId = businessUnitId,
				PersonId = personId
			});

			AnalyticsSchedules.FactScheduleRows.SingleOrDefault().Should().Not.Be.Null();
		}

		[Test]
		public void ShouldNotTryToSaveScheduleChangeWhenPersonPeriodMissingFromAnalytics()
		{
			AnalyticsDates.HasDatesBetween(new DateTime(1999, 12, 31), new DateTime(2030, 12, 31));
			var businessUnit = BusinessUnitFactory.CreateSimpleBusinessUnit("Default BU").WithId(businessUnitId);
			var scenario = ScenarioFactory.CreateScenario("Default", true, true).WithId(scenarioId);
			var person = PersonFactory.CreatePerson().WithId(personId);
			var personPeriod = PersonPeriodFactory.CreatePersonPeriod(new DateOnly(startDate)).WithId(personPeriodId);
			personPeriod.Team = TeamFactory.CreateTeam("Team", "Site");
			person.AddPersonPeriod(personPeriod);
			var assignment = PersonAssignmentFactory.CreateEmptyAssignment(person, scenario,
				new DateTimePeriod(startDate, endDate));

			var analyticsScenario = new AnalyticsScenario
			{
				ScenarioCode = scenarioId, ScenarioId = 6
			};
			var scheduleDay = new ProjectionChangedEventScheduleDay
			{
				Date = startDate.Date,
				PersonPeriodId = personPeriodId
			};

			BusinessUnits.Add(businessUnit);
			Scenarios.Add(scenario);
			Persons.Add(person);
			Schedules.Add(assignment);

			AnalyticsScenarios.AddScenario(analyticsScenario);

			Assert.Throws<PersonPeriodMissingInAnalyticsException>(() => Target.Handle(new ProjectionChangedEvent
			{
				ScheduleDays = new Collection<ProjectionChangedEventScheduleDay> {scheduleDay},
				ScenarioId = scenarioId,
				LogOnBusinessUnitId = businessUnitId,
				PersonId = personId
			}));
		}

		[Test]
		public void ShouldSaveFactScheduleAndFactScheduleDayCountFromApplicationDatabase()
		{
			AnalyticsDates.HasDatesBetween(new DateTime(1999, 12, 31), new DateTime(2030, 12, 31));
			var shiftCategoryId = Guid.NewGuid();
			var businessUnit = BusinessUnitFactory.CreateSimpleBusinessUnit("Default BU").WithId(businessUnitId);
			var scenario = ScenarioFactory.CreateScenario("Default", true, true).WithId(scenarioId);
			var person = PersonFactory.CreatePerson().WithId(personId);
			var personPeriod = PersonPeriodFactory.CreatePersonPeriod(new DateOnly(startDate)).WithId(personPeriodId);
			personPeriod.Team = TeamFactory.CreateTeam("Team", "Site");
			person.AddPersonPeriod(personPeriod);
			var assignment = PersonAssignmentFactory.CreateAssignmentWithMainShift(person, scenario,
				new DateTimePeriod(startDate, endDate));
			assignment.ShiftCategory.SetId(shiftCategoryId);

			var scheduleDay = new ProjectionChangedEventScheduleDay
			{
				ShiftCategoryId = Guid.Empty, // This category should not be used
				PersonPeriodId = personPeriodId,
				Date = startDate.Date
			};
			var cat = new AnalyticsShiftCategory
			{
				ShiftCategoryId = 1,
				ShiftCategoryCode = shiftCategoryId
			};
			var analyticsScenario = new AnalyticsScenario
			{
				ScenarioCode = scenarioId, ScenarioId = 66
			};

			BusinessUnits.Add(businessUnit);
			Scenarios.Add(scenario);
			Persons.Add(person);
			Schedules.Add(assignment);

			AnalyticsScenarios.AddScenario(analyticsScenario);
			AnalyticsShiftCategories.AddShiftCategory(cat);
			AnalyticsPersonPeriods.AddOrUpdatePersonPeriod(new AnalyticsPersonPeriod
			{
				PersonId = 11,
				BusinessUnitId = 2,
				PersonPeriodCode = personPeriodId
			});
			IntervalLength.Has(intervalLengthInMinute);

			Target.Handle(new ProjectionChangedEvent
			{
				ScheduleDays = new Collection<ProjectionChangedEventScheduleDay> {scheduleDay},
				ScenarioId = scenarioId,
				PersonId = personId,
				LogOnBusinessUnitId = businessUnitId
			});

			AnalyticsSchedules.FactScheduleRows.Count.Should().Be.EqualTo(9 * 4); // 9 hours of activity and 15 minute intervals
			AnalyticsSchedules.FactScheduleRows.ForEach(row => row.PersonPart.PersonId.Should().Be.EqualTo(11));
			AnalyticsSchedules.FactScheduleRows.ForEach(row => row.PersonPart.BusinessUnitId.Should().Be.EqualTo(2));
			AnalyticsSchedules.FactScheduleRows.ForEach(row => row.TimePart.ScenarioId.Should().Be.EqualTo(66));
			AnalyticsSchedules.FactScheduleRows.ForEach(row => row.TimePart.ShiftCategoryId.Should().Be.EqualTo(1));

			AnalyticsSchedules.FactScheduleDayCountRows.Count.Should().Be.EqualTo(1);
			AnalyticsSchedules.FactScheduleDayCountRows.Single().PersonId.Should().Be.EqualTo(11);
			AnalyticsSchedules.FactScheduleDayCountRows.Single().AbsenceId.Should().Be.EqualTo(-1);
			AnalyticsSchedules.FactScheduleDayCountRows.Single().BusinessUnitId.Should().Be.EqualTo(2);
			AnalyticsSchedules.FactScheduleDayCountRows.Single().ScenarioId.Should().Be.EqualTo(66);
			AnalyticsSchedules.FactScheduleDayCountRows.Single().ShiftCategoryId.Should().Be.EqualTo(1);
			AnalyticsSchedules.FactScheduleDayCountRows.Single().StartTime.Should().Be.EqualTo(startDate);
			AnalyticsSchedules.FactScheduleDayCountRows.Single().DayOffName.Should().Be.Null();
			AnalyticsSchedules.FactScheduleDayCountRows.Single().DayOffShortName.Should().Be.Null();
		}

		[Test]
		public void ShouldOnlyDeleteScheduleIfNotScheduled()
		{
			AnalyticsDates.HasDatesBetween(new DateTime(1999, 12, 31), new DateTime(2030, 12, 31));
			var businessUnit = BusinessUnitFactory.CreateSimpleBusinessUnit("Default BU").WithId(businessUnitId);
			var scenario = ScenarioFactory.CreateScenario("Default", true, true).WithId(scenarioId);
			var person = PersonFactory.CreatePerson().WithId(personId);
			var personPeriod = PersonPeriodFactory.CreatePersonPeriod(new DateOnly(startDate)).WithId(personPeriodId);
			personPeriod.Team = TeamFactory.CreateTeam("Team", "Site");
			person.AddPersonPeriod(personPeriod);

			var analyticsScenario = new AnalyticsScenario
			{
				ScenarioCode = scenarioId, ScenarioId = 6
			};
			var scheduleDay = new ProjectionChangedEventScheduleDay
			{
				Date = startDate.Date,
				NotScheduled = true
			};
			var analyticsPersonPeriod = new AnalyticsPersonPeriod
			{
				PersonId = 55,
				BusinessUnitId = 2,
				PersonPeriodCode = personPeriodId,
				PersonCode = personId
			};

			var existingSchedule = new FactScheduleRow
			{
				DatePart = new AnalyticsFactScheduleDate
				{
					ScheduleDateId = AnalyticsDates.Date(scheduleDay.Date).DateId
				},
				PersonPart = new AnalyticsFactSchedulePerson
				{
					BusinessUnitId = analyticsPersonPeriod.BusinessUnitId,
					PersonId = analyticsPersonPeriod.PersonId
				},
				TimePart = new AnalyticsFactScheduleTime
				{
					ScenarioId = analyticsScenario.ScenarioId
				}
			};
			var existingScheduleDayCount = new AnalyticsFactScheduleDayCount
			{
				PersonId = analyticsPersonPeriod.PersonId,
				ShiftStartDateLocalId = AnalyticsDates.Date(scheduleDay.Date).DateId,
				ScenarioId = analyticsScenario.ScenarioId
			};

			BusinessUnits.Add(businessUnit);
			Scenarios.Add(scenario);
			Persons.Add(person);

			AnalyticsPersonPeriods.AddOrUpdatePersonPeriod(analyticsPersonPeriod);
			AnalyticsScenarios.AddScenario(analyticsScenario);

			AnalyticsSchedules.PersistFactScheduleDayCountRow(existingScheduleDayCount);
			AnalyticsSchedules.PersistFactScheduleBatch(new List<IFactScheduleRow> {existingSchedule});

			Target.Handle(new ProjectionChangedEvent
			{
				ScheduleDays = new Collection<ProjectionChangedEventScheduleDay> {scheduleDay},
				ScenarioId = scenarioId,
				PersonId = personId,
				LogOnBusinessUnitId = businessUnitId
			});

			AnalyticsSchedules.FactScheduleRows.Should().Be.Empty();
			AnalyticsSchedules.FactScheduleDayCountRows.Should().Be.Empty();
		}

		[Test]
		public void ShouldPersistScheduleDayCountWhenDayOff()
		{
			AnalyticsDates.HasDatesBetween(new DateTime(1999, 12, 31), new DateTime(2030, 12, 31));
			var businessUnit = BusinessUnitFactory.CreateSimpleBusinessUnit("Default BU").WithId(businessUnitId);
			var scenario = ScenarioFactory.CreateScenario("Default", true, true).WithId(scenarioId);
			var person = PersonFactory.CreatePerson().WithId(personId);
			var personPeriod = PersonPeriodFactory.CreatePersonPeriod(new DateOnly(startDate)).WithId(personPeriodId);
			personPeriod.Team = TeamFactory.CreateTeam("Team", "Site");
			person.AddPersonPeriod(personPeriod);
			var dayOffTemplate = DayOffFactory.CreateDayOff(new Description("Slackday", "SD"));
			var assignment = PersonAssignmentFactory.CreateAssignmentWithDayOff(person, scenario,
				new DateOnly(startDate), dayOffTemplate);

			var shift = new ProjectionChangedEventShift
			{
				StartDateTime = startDate,
				EndDateTime = startDate.AddHours(2).AddMinutes(15)
			};

			var scheduleDay = new ProjectionChangedEventScheduleDay
			{
				Shift = shift,
				PersonPeriodId = personPeriodId,
				Date = startDate.Date
			};
			var analyticsScenario = new AnalyticsScenario
			{
				ScenarioCode = scenarioId, ScenarioId = 66
			};

			BusinessUnits.Add(businessUnit);
			Scenarios.Add(scenario);
			Persons.Add(person);
			Schedules.Add(assignment);

			AnalyticsScenarios.AddScenario(analyticsScenario);
			AnalyticsPersonPeriods.AddOrUpdatePersonPeriod(new AnalyticsPersonPeriod
			{
				PersonId = 11,
				BusinessUnitId = 2,
				PersonPeriodCode = personPeriodId
			});
			IntervalLength.Has(intervalLengthInMinute);

			Target.Handle(new ProjectionChangedEvent
			{
				ScheduleDays = new Collection<ProjectionChangedEventScheduleDay> {scheduleDay},
				ScenarioId = scenarioId,
				PersonId = personId,
				LogOnBusinessUnitId = businessUnitId
			});

			AnalyticsSchedules.FactScheduleRows.Count.Should().Be.EqualTo(0);

			AnalyticsSchedules.FactScheduleDayCountRows.Count.Should().Be.EqualTo(1);
			AnalyticsSchedules.FactScheduleDayCountRows.Single().PersonId.Should().Be.EqualTo(11);
			AnalyticsSchedules.FactScheduleDayCountRows.Single().AbsenceId.Should().Be.EqualTo(-1);
			AnalyticsSchedules.FactScheduleDayCountRows.Single().BusinessUnitId.Should().Be.EqualTo(2);
			AnalyticsSchedules.FactScheduleDayCountRows.Single().ScenarioId.Should().Be.EqualTo(66);
			AnalyticsSchedules.FactScheduleDayCountRows.Single().ShiftCategoryId.Should().Be.EqualTo(-1);
			AnalyticsSchedules.FactScheduleDayCountRows.Single().StartTime.Should().Be.EqualTo(startDate.Date);
			AnalyticsSchedules.FactScheduleDayCountRows.Single().DayOffName.Should().Be.EqualTo(dayOffTemplate.Description.Name);
			AnalyticsSchedules.FactScheduleDayCountRows.Single().DayOffShortName.Should().Be
				.EqualTo(dayOffTemplate.Description.ShortName);
		}

		[Test]
		public void ShouldAddDayOffIfNotExistsWhenPersistScheduleDayCount()
		{
			AnalyticsDates.HasDatesBetween(new DateTime(1999, 12, 31), new DateTime(2030, 12, 31));
			var businessUnit = BusinessUnitFactory.CreateSimpleBusinessUnit("Default BU").WithId(businessUnitId);
			var scenario = ScenarioFactory.CreateScenario("Default", true, true).WithId(scenarioId);
			var person = PersonFactory.CreatePerson().WithId(personId);
			var personPeriod = PersonPeriodFactory.CreatePersonPeriod(new DateOnly(startDate)).WithId(personPeriodId);
			personPeriod.Team = TeamFactory.CreateTeam("Team", "Site");
			person.AddPersonPeriod(personPeriod);
			var dayOffTemplate = DayOffFactory.CreateDayOff(new Description("Slackday", "SD")).WithId(Guid.NewGuid());
			var assignment = PersonAssignmentFactory.CreateAssignmentWithDayOff(person, scenario,
				new DateOnly(startDate), dayOffTemplate);

			var shift = new ProjectionChangedEventShift
			{
				StartDateTime = startDate,
				EndDateTime = startDate.AddHours(2).AddMinutes(15)
			};

			var scheduleDay = new ProjectionChangedEventScheduleDay
			{
				Shift = shift,
				PersonPeriodId = personPeriodId,
				Date = startDate.Date
			};
			var analyticsScenario = new AnalyticsScenario
			{
				ScenarioCode = scenarioId, ScenarioId = 66
			};

			BusinessUnits.Add(businessUnit);
			Scenarios.Add(scenario);
			Persons.Add(person);
			Schedules.Add(assignment);

			AnalyticsScenarios.AddScenario(analyticsScenario);
			AnalyticsPersonPeriods.AddOrUpdatePersonPeriod(new AnalyticsPersonPeriod
			{
				PersonId = 11,
				BusinessUnitId = 2,
				PersonPeriodCode = personPeriodId
			});
			IntervalLength.Has(intervalLengthInMinute);

			Target.Handle(new ProjectionChangedEvent
			{
				ScheduleDays = new Collection<ProjectionChangedEventScheduleDay> {scheduleDay},
				ScenarioId = scenarioId,
				PersonId = personId,
				LogOnBusinessUnitId = businessUnitId
			});

			var analyticsDayOff = AnalyticsDayOffRepository.DayOffs().Single();
			analyticsDayOff.DayOffName.Should().Be.EqualTo(dayOffTemplate.Description.Name);
			analyticsDayOff.DayOffShortname.Should().Be.EqualTo(dayOffTemplate.Description.ShortName);
			analyticsDayOff.BusinessUnitId.Should().Be.EqualTo(2);
			analyticsDayOff.DayOffCode.Should().Be.EqualTo(dayOffTemplate.Id.Value);
		}

		[Test]
		public void ShouldPersistScheduleDayCountWhenFullDayAbsence()
		{
			AnalyticsDates.HasDatesBetween(new DateTime(1999, 12, 31), new DateTime(2030, 12, 31));
			var businessUnit = BusinessUnitFactory.CreateSimpleBusinessUnit("Default BU").WithId(businessUnitId);
			var scenario = ScenarioFactory.CreateScenario("Default", true, true).WithId(scenarioId);
			var person = PersonFactory.CreatePerson().WithId(personId);
			var personPeriod = PersonPeriodFactory.CreatePersonPeriod(new DateOnly(startDate)).WithId(personPeriodId);
			personPeriod.Team = TeamFactory.CreateTeam("Team", "Site");
			person.AddPersonPeriod(personPeriod);

			var absence = AbsenceFactory.CreateAbsence("FullDayAbsence").WithId(absenceId);
			var personAbsence = PersonAbsenceFactory.CreatePersonAbsence(person, scenario,
				new DateTimePeriod(startDate, endDate), absence);

			var shift = new ProjectionChangedEventShift
			{
				StartDateTime = startDate,
				EndDateTime = startDate.AddHours(2).AddMinutes(15)
			};

			var scheduleDay = new ProjectionChangedEventScheduleDay
			{
				Shift = shift,
				PersonPeriodId = personPeriodId,
				Date = startDate.Date
			};
			var analyticsScenario = new AnalyticsScenario
			{
				ScenarioCode = scenarioId, ScenarioId = 66
			};
			var analyticsAbsence = new AnalyticsAbsence
			{
				AbsenceId = 123,
				AbsenceCode = absenceId
			};

			BusinessUnits.Add(businessUnit);
			Scenarios.Add(scenario);
			Persons.Add(person);
			Schedules.Add(personAbsence);

			AnalyticsAbsences.AddAbsence(analyticsAbsence);
			AnalyticsScenarios.AddScenario(analyticsScenario);
			AnalyticsPersonPeriods.AddOrUpdatePersonPeriod(new AnalyticsPersonPeriod
			{
				PersonId = 11,
				BusinessUnitId = 2,
				PersonPeriodCode = personPeriodId
			});
			IntervalLength.Has(intervalLengthInMinute);

			Target.Handle(new ProjectionChangedEvent
			{
				ScheduleDays = new Collection<ProjectionChangedEventScheduleDay> {scheduleDay},
				ScenarioId = scenarioId,
				PersonId = personId,
				LogOnBusinessUnitId = businessUnitId
			});

			// 8 hours of activity and 15 minute intervals (full day = 8 hours even though its actually saved as 9)
			AnalyticsSchedules.FactScheduleRows.Count.Should().Be.EqualTo(8 * 4);
			AnalyticsSchedules.FactScheduleRows.ForEach(row => row.PersonPart.PersonId.Should().Be.EqualTo(11));
			AnalyticsSchedules.FactScheduleRows.ForEach(row => row.PersonPart.BusinessUnitId.Should().Be.EqualTo(2));
			AnalyticsSchedules.FactScheduleRows.ForEach(row => row.TimePart.ScenarioId.Should().Be.EqualTo(66));
			AnalyticsSchedules.FactScheduleRows.ForEach(row => row.TimePart.ShiftCategoryId.Should().Be.EqualTo(-1));
			AnalyticsSchedules.FactScheduleRows.ForEach(row =>
				row.TimePart.AbsenceId.Should().Be.EqualTo(analyticsAbsence.AbsenceId));

			AnalyticsSchedules.FactScheduleDayCountRows.Count.Should().Be.EqualTo(1);
			AnalyticsSchedules.FactScheduleDayCountRows.Single().PersonId.Should().Be.EqualTo(11);
			AnalyticsSchedules.FactScheduleDayCountRows.Single().AbsenceId.Should().Be.EqualTo(analyticsAbsence.AbsenceId);
			AnalyticsSchedules.FactScheduleDayCountRows.Single().BusinessUnitId.Should().Be.EqualTo(2);
			AnalyticsSchedules.FactScheduleDayCountRows.Single().ScenarioId.Should().Be.EqualTo(66);
			AnalyticsSchedules.FactScheduleDayCountRows.Single().ShiftCategoryId.Should().Be.EqualTo(-1);
			AnalyticsSchedules.FactScheduleDayCountRows.Single().StartTime.Should().Be.EqualTo(startDate);
			AnalyticsSchedules.FactScheduleDayCountRows.Single().DayOffName.Should().Be.Null();
			AnalyticsSchedules.FactScheduleDayCountRows.Single().DayOffShortName.Should().Be.Null();
		}

		[Test]
		public void ShouldNotBeHandledScenarioDoesNotExistInAnalytics()
		{
			AnalyticsDates.HasDatesBetween(new DateTime(1999, 12, 31), new DateTime(2030, 12, 31));
			var businessUnit = BusinessUnitFactory.CreateSimpleBusinessUnit("Default BU").WithId(businessUnitId);
			var scenario = ScenarioFactory.CreateScenario("Default", true, true).WithId(scenarioId);
			var person = PersonFactory.CreatePerson().WithId(personId);
			var personPeriod = PersonPeriodFactory.CreatePersonPeriod(new DateOnly(startDate)).WithId(personPeriodId);
			personPeriod.Team = TeamFactory.CreateTeam("Team", "Site");
			person.AddPersonPeriod(personPeriod);
			var dayOffTemplate = DayOffFactory.CreateDayOff(new Description("Slackday", "SD"));
			var assignment = PersonAssignmentFactory.CreateAssignmentWithDayOff(person, scenario,
				new DateOnly(startDate), dayOffTemplate);

			var shift = new ProjectionChangedEventShift
			{
				StartDateTime = startDate,
				EndDateTime = startDate.AddHours(2).AddMinutes(15)
			};

			var scheduleDay = new ProjectionChangedEventScheduleDay
			{
				Shift = shift,
				PersonPeriodId = personPeriodId,
				Date = startDate.Date
			};

			BusinessUnits.Add(businessUnit);
			Scenarios.Add(scenario);
			Persons.Add(person);
			Schedules.Add(assignment);

			AnalyticsPersonPeriods.AddOrUpdatePersonPeriod(new AnalyticsPersonPeriod
			{
				PersonId = 11,
				BusinessUnitId = 2,
				PersonPeriodCode = personPeriodId
			});
			IntervalLength.Has(intervalLengthInMinute);

			Assert.Throws<ScenarioMissingInAnalyticsException>(() => Target.Handle(new ProjectionChangedEvent
			{
				ScheduleDays = new Collection<ProjectionChangedEventScheduleDay> {scheduleDay},
				ScenarioId = scenarioId,
				PersonId = personId,
				LogOnBusinessUnitId = businessUnitId
			}));
		}

		[Test]
		[Toggle(Toggles.ETL_SpeedUpFactScheduleNightly_38019)]
		public void ShouldNotBeHandledBecauseFilterWhenNotDefaultScenarioAndNotReportable()
		{
			AnalyticsDates.HasDatesBetween(new DateTime(1999, 12, 31), new DateTime(2030, 12, 31));
			var businessUnit = BusinessUnitFactory.CreateSimpleBusinessUnit("Default BU").WithId(businessUnitId);
			var scenario = ScenarioFactory.CreateScenario("Default", false, false).WithId(scenarioId);

			var scheduleDay = new ProjectionChangedEventScheduleDay
			{
				DayOff = new ProjectionChangedEventDayOff(),
				PersonPeriodId = Guid.NewGuid()
			};

			BusinessUnits.Add(businessUnit);
			Scenarios.Add(scenario);

			Assert.DoesNotThrow(() => Target.Handle(new ProjectionChangedEvent
			{
				ScheduleDays = new Collection<ProjectionChangedEventScheduleDay> {scheduleDay},
				ScenarioId = scenarioId,
				IsDefaultScenario = false,
				LogOnBusinessUnitId = businessUnitId
			}));
		}

		[Test]
		public void ShouldContinueSaveScheduleChangeWhenAnyDateIdMappingFails()
		{
			var scheduleStartDate = new DateTime(2018, 01, 01, 8, 0, 0, DateTimeKind.Utc);
			var scheduleEndDate = new DateTime(2018, 01, 01, 17, 0, 0, DateTimeKind.Utc);

			AnalyticsDates.HasDatesBetween(new DateTime(2018, 01, 01), new DateTime(2018, 12, 31));

			var businessUnit = BusinessUnitFactory.CreateSimpleBusinessUnit("Default BU").WithId(businessUnitId);
			BusinessUnits.Add(businessUnit);

			var scenario = ScenarioFactory.CreateScenario("Default", true, true).WithId(scenarioId);
			Scenarios.Add(scenario);

			var analyticsScenario = new AnalyticsScenario
			{
				ScenarioCode = scenarioId,
				ScenarioId = 66
			};
			AnalyticsScenarios.AddScenario(analyticsScenario);

			var person = PersonFactory.CreatePerson().WithId(personId);
			var personPeriod = PersonPeriodFactory.CreatePersonPeriod(new DateOnly(startDate)).WithId(personPeriodId);
			personPeriod.Team = TeamFactory.CreateTeam("Team", "Site");
			person.AddPersonPeriod(personPeriod);

			var shiftCategoryId = Guid.NewGuid();

			var shiftCategory = new AnalyticsShiftCategory
			{
				ShiftCategoryId = 1,
				ShiftCategoryCode = shiftCategoryId
			};
			AnalyticsShiftCategories.AddShiftCategory(shiftCategory);

			var assignment1Period = new DateTimePeriod(scheduleStartDate.AddDays(-1), scheduleEndDate.AddDays(-1));
			var assignment1 = PersonAssignmentFactory.CreateAssignmentWithMainShift(person, scenario, assignment1Period);
			assignment1.ShiftCategory.SetId(shiftCategoryId);
			Schedules.Add(assignment1);

			var assignment2Period = new DateTimePeriod(scheduleStartDate, scheduleEndDate);
			var assignment2 = PersonAssignmentFactory.CreateAssignmentWithMainShift(person, scenario, assignment2Period);
			assignment2.ShiftCategory.SetId(shiftCategoryId);
			Schedules.Add(assignment2);

			var dayOffTemplate = DayOffFactory.CreateDayOff(new Description("Slackday", "SD"));
			var assignmentDayOff = PersonAssignmentFactory.CreateAssignmentWithDayOff(person, scenario,
				new DateOnly(scheduleStartDate.AddDays(1)), dayOffTemplate);
			Schedules.Add(assignmentDayOff);

			Persons.Add(person);

			AnalyticsPersonPeriods.AddOrUpdatePersonPeriod(new AnalyticsPersonPeriod
			{
				PersonId = 11,
				BusinessUnitId = 2,
				PersonPeriodCode = personPeriodId
			});
			IntervalLength.Has(intervalLengthInMinute);

			var scheduleDate = scheduleStartDate.AddDays(-1);
			var scheduleDay1 = new ProjectionChangedEventScheduleDay
			{
				PersonPeriodId = personPeriodId,
				Date = scheduleDate.Date
			};

			scheduleDate = scheduleStartDate;
			var scheduleDay2 = new ProjectionChangedEventScheduleDay
			{
				Shift = new ProjectionChangedEventShift
				{
					StartDateTime = scheduleDate,
					EndDateTime = scheduleDate.AddHours(3).AddMinutes(30)
				},
				PersonPeriodId = personPeriodId,
				Date = scheduleDate.Date
			};

			scheduleDate = scheduleStartDate.AddDays(1);
			var scheduleDay3 = new ProjectionChangedEventScheduleDay
			{
				Shift = new ProjectionChangedEventShift
				{
					StartDateTime = scheduleDate,
					EndDateTime = scheduleDate.AddHours(4).AddMinutes(45)
				},
				PersonPeriodId = personPeriodId,
				Date = scheduleDate.Date
			};

			AnalyticsScenarios.AddScenario(analyticsScenario);
			AnalyticsPersonPeriods.AddOrUpdatePersonPeriod(new AnalyticsPersonPeriod
			{
				PersonId = 11,
				BusinessUnitId = 2,
				PersonPeriodCode = personPeriodId
			});
			IntervalLength.Has(intervalLengthInMinute);

			// 2017-12-31 is out of analytics date range, schedule change for it will not be processed
			Target.Handle(new ProjectionChangedEvent
			{
				ScheduleDays = new Collection<ProjectionChangedEventScheduleDay>
				{
					scheduleDay1,
					scheduleDay2,
					scheduleDay3
				},
				ScenarioId = scenarioId,
				PersonId = personId,
				LogOnBusinessUnitId = businessUnitId
			});

			var factScheduleDays = AnalyticsSchedules.FactScheduleDayCountRows;
			factScheduleDays.Count.Should().Be.EqualTo(2);
			factScheduleDays.First().StartTime.Should().Be.EqualTo(scheduleDay2.Shift.StartDateTime);
			// Full DayOff, start from 00:00
			factScheduleDays.Second().StartTime.Should().Be.EqualTo(scheduleDay3.Shift.StartDateTime.Date);

			AnalyticsDayOffRepository.DayOffs().Count.Should().Be.EqualTo(1);
			AnalyticsSchedules.FactScheduleRows.Count.Should().Be
				.EqualTo((int) assignment2Period.ElapsedTime().TotalMinutes / intervalLengthInMinute);

			foreach (var row in AnalyticsSchedules.FactScheduleRows)
			{
				row.DatePart.ShiftStartTime.Should().Be.EqualTo(scheduleStartDate);
			}
		}

		[Test]
		public void ShouldContinueSaveScheduleChangeWhenAnyScheduleDayDeleted()
		{
			AnalyticsDates.HasDatesBetween(new DateTime(1999, 12, 31), new DateTime(2030, 12, 31));

			var businessUnit = BusinessUnitFactory.CreateSimpleBusinessUnit("Default BU").WithId(businessUnitId);
			BusinessUnits.Add(businessUnit);

			var scenario = ScenarioFactory.CreateScenario("Default", true, true).WithId(scenarioId);
			Scenarios.Add(scenario);

			var analyticsScenario = new AnalyticsScenario
			{
				ScenarioCode = scenarioId,
				ScenarioId = 66
			};
			AnalyticsScenarios.AddScenario(analyticsScenario);

			var person = PersonFactory.CreatePerson().WithId(personId);
			var personPeriod = PersonPeriodFactory.CreatePersonPeriod(new DateOnly(startDate.AddDays(-30))).WithId(personPeriodId);
			personPeriod.Team = TeamFactory.CreateTeam("Team", "Site");
			person.AddPersonPeriod(personPeriod);

			var shiftCategoryId = Guid.NewGuid();

			var shiftCategory = new AnalyticsShiftCategory
			{
				ShiftCategoryId = 1,
				ShiftCategoryCode = shiftCategoryId
			};
			AnalyticsShiftCategories.AddShiftCategory(shiftCategory);

			// Assignment for 2017-03-07
			var assignmentPeriod = new DateTimePeriod(startDate, endDate);
			var assignment = PersonAssignmentFactory.CreateAssignmentWithMainShift(person, scenario, assignmentPeriod);
			assignment.ShiftCategory.SetId(shiftCategoryId);
			Schedules.Add(assignment);

			// DayOff assignment for 2017-03-08
			var dayOffTemplate = DayOffFactory.CreateDayOff(new Description("Slackday", "SD"));
			var assignmentDayOff = PersonAssignmentFactory.CreateAssignmentWithDayOff(person, scenario,
				new DateOnly(startDate.AddDays(1)), dayOffTemplate);
			Schedules.Add(assignmentDayOff);

			Persons.Add(person);

			AnalyticsPersonPeriods.AddOrUpdatePersonPeriod(new AnalyticsPersonPeriod
			{
				PersonId = 11,
				BusinessUnitId = 2,
				PersonPeriodCode = personPeriodId
			});
			IntervalLength.Has(intervalLengthInMinute);

			var scheduleDate = startDate.AddDays(-1);
			var scheduleDay1 = new ProjectionChangedEventScheduleDay
			{
				Shift = new ProjectionChangedEventShift
				{
					StartDateTime = scheduleDate,
					EndDateTime = scheduleDate.AddHours(2).AddMinutes(15)
				},
				PersonPeriodId = personPeriodId,
				Date = scheduleDate.Date
			};

			scheduleDate = startDate;
			var scheduleDay2 = new ProjectionChangedEventScheduleDay
			{
				Shift = new ProjectionChangedEventShift
				{
					StartDateTime = scheduleDate,
					EndDateTime = scheduleDate.AddHours(3).AddMinutes(30)
				},
				PersonPeriodId = personPeriodId,
				Date = scheduleDate.Date
			};

			scheduleDate = startDate.AddDays(1);
			var scheduleDay3 = new ProjectionChangedEventScheduleDay
			{
				Shift = new ProjectionChangedEventShift
				{
					StartDateTime = scheduleDate,
					EndDateTime = scheduleDate.AddHours(4).AddMinutes(45)
				},
				PersonPeriodId = personPeriodId,
				Date = scheduleDate.Date
			};

			AnalyticsScenarios.AddScenario(analyticsScenario);
			AnalyticsPersonPeriods.AddOrUpdatePersonPeriod(new AnalyticsPersonPeriod
			{
				PersonId = 11,
				BusinessUnitId = 2,
				PersonPeriodCode = personPeriodId
			});
			IntervalLength.Has(intervalLengthInMinute);

			// No assignment for 2017-03-06, so schedule will be deleted for that day
			Target.Handle(new ProjectionChangedEvent
			{
				ScheduleDays = new Collection<ProjectionChangedEventScheduleDay>
				{
					scheduleDay1,
					scheduleDay2,
					scheduleDay3
				},
				ScenarioId = scenarioId,
				PersonId = personId,
				LogOnBusinessUnitId = businessUnitId
			});

			var factScheduleDays = AnalyticsSchedules.FactScheduleDayCountRows;
			factScheduleDays.Count.Should().Be.EqualTo(2);
			factScheduleDays.First().StartTime.Should().Be.EqualTo(scheduleDay2.Shift.StartDateTime);
			// Full DayOff, start from 00:00
			factScheduleDays.Second().StartTime.Should().Be.EqualTo(scheduleDay3.Shift.StartDateTime.Date);

			AnalyticsDayOffRepository.DayOffs().Count.Should().Be.EqualTo(1);
			AnalyticsSchedules.FactScheduleRows.Count.Should().Be
				.EqualTo((int)assignmentPeriod.ElapsedTime().TotalMinutes / intervalLengthInMinute);

			foreach (var row in AnalyticsSchedules.FactScheduleRows)
			{
				row.DatePart.ShiftStartTime.Should().Be.EqualTo(startDate);
			}
		}
	}
}