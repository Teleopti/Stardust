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

		private readonly Guid businessUnitId = Guid.NewGuid();
		private readonly Guid scenarioId = Guid.NewGuid();
		private readonly Guid personId = Guid.NewGuid();
		private readonly Guid personPeriodId = Guid.NewGuid();
		private readonly Guid absenceId = Guid.NewGuid();
		private readonly DateTime startDate = new DateTime(2017, 03, 07, 8, 0, 0, DateTimeKind.Utc);
		private readonly DateTime endDate = new DateTime(2017, 03, 07, 17, 0, 0, DateTimeKind.Utc);

		public void Setup(ISystem system, IIocConfiguration configuration)
		{
			system.AddService<FakeIntervalLengthFetcher>();
			system.AddService<FakeAnalyticsScheduleRepository>();
			system.AddService<FakeAnalyticsShiftCategoryRepository>();
			system.AddService<FakeAnalyticsAbsenceRepository>();
			system.AddService<FakeAnalyticsActivityRepository>();
			system.AddService<FakeAnalyticsOvertimeRepository>();
			system.AddService<FakeAnalyticsScenarioRepository>();
			system.AddService<FakeAnalyticsPersonPeriodRepository>();
			system.UseTestDouble<FakeScheduleStorage>().For<IScheduleStorage>();
			system.AddService(new FakeAnalyticsDateRepository(new DateTime(1999, 12, 31), new DateTime(2030, 12, 31)));
			system.AddService<AnalyticsScheduleChangeUpdater>();
		}

		[Test]
		public void ShouldNotTryToSaveScheduleChangeWhenDateIdMappingFails()
		{
			var businessUnit = BusinessUnitFactory.CreateSimpleBusinessUnit("Default BU").WithId(businessUnitId);
			var scenario = ScenarioFactory.CreateScenario("Default", true, true).WithId(scenarioId);
			var person = PersonFactory.CreatePerson().WithId(personId);
			var personPeriod = PersonPeriodFactory.CreatePersonPeriod(new DateOnly(startDate)).WithId(personPeriodId);
			personPeriod.Team = TeamFactory.CreateTeam("Team", "Site");
			person.AddPersonPeriod(personPeriod);
			var assignment = PersonAssignmentFactory.CreateEmptyAssignment(person, scenario, new DateTimePeriod(startDate, endDate));
			
			var analyticsScenario = new AnalyticsScenario { ScenarioCode = scenarioId, ScenarioId = 3};
			var scheduleDay = new ProjectionChangedEventScheduleDay
			{
				Date = startDate.Date,
				PersonPeriodId = personPeriodId
			};

			var analyticsPersonPeriod = new AnalyticsPersonPeriod { PersonId = 1, BusinessUnitId = 2, PersonPeriodCode = scheduleDay.PersonPeriodId };
			var existingSchedule = new FactScheduleRow
			{
				DatePart = new AnalyticsFactScheduleDate { ScheduleDateId = AnalyticsDates.Date(scheduleDay.Date).DateId },
				PersonPart = new AnalyticsFactSchedulePerson { BusinessUnitId = analyticsPersonPeriod.BusinessUnitId, PersonId = analyticsPersonPeriod.PersonId},
				TimePart = new AnalyticsFactScheduleTime { ScenarioId = analyticsScenario.ScenarioId}
			};

			BusinessUnits.Add(businessUnit);
			Scenarios.Add(scenario);
			Persons.Add(person);
			Schedules.Add(assignment);

			AnalyticsScenarios.AddScenario(analyticsScenario);
			AnalyticsPersonPeriods.AddPersonPeriod(analyticsPersonPeriod);
			AnalyticsSchedules.PersistFactScheduleBatch(new List<IFactScheduleRow> { existingSchedule });
			AnalyticsDates.Clear();

			Target.Handle(new ProjectionChangedEvent
			{
				ScheduleDays = new Collection<ProjectionChangedEventScheduleDay> { scheduleDay },
				ScenarioId = scenarioId,
				LogOnBusinessUnitId = businessUnitId,
				PersonId = personId
			});

			AnalyticsSchedules.FactScheduleRows.SingleOrDefault().Should().Not.Be.Null();
		}

		[Test]
		public void ShouldNotTryToSaveScheduleChangeWhenPersonPeriodMissingFromAnalytics()
		{
			var businessUnit = BusinessUnitFactory.CreateSimpleBusinessUnit("Default BU").WithId(businessUnitId);
			var scenario = ScenarioFactory.CreateScenario("Default", true, true).WithId(scenarioId);
			var person = PersonFactory.CreatePerson().WithId(personId);
			var personPeriod = PersonPeriodFactory.CreatePersonPeriod(new DateOnly(startDate)).WithId(personPeriodId);
			personPeriod.Team = TeamFactory.CreateTeam("Team", "Site");
			person.AddPersonPeriod(personPeriod);
			var assignment = PersonAssignmentFactory.CreateEmptyAssignment(person, scenario, new DateTimePeriod(startDate, endDate));

			var analyticsScenario = new AnalyticsScenario { ScenarioCode = scenarioId, ScenarioId = 6 };
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
				ScheduleDays = new Collection<ProjectionChangedEventScheduleDay> { scheduleDay },
				ScenarioId = scenarioId,
				LogOnBusinessUnitId = businessUnitId,
				PersonId = personId
			}));
		}

		[Test]
		public void ShouldSaveFactScheduleAndFactScheduleDayCountFromApplicationDatabase()
		{
			var shiftCategoryId = Guid.NewGuid();
			var businessUnit = BusinessUnitFactory.CreateSimpleBusinessUnit("Default BU").WithId(businessUnitId);
			var scenario = ScenarioFactory.CreateScenario("Default", true, true).WithId(scenarioId);
			var person = PersonFactory.CreatePerson().WithId(personId);
			var personPeriod = PersonPeriodFactory.CreatePersonPeriod(new DateOnly(startDate)).WithId(personPeriodId);
			personPeriod.Team = TeamFactory.CreateTeam("Team", "Site");
			person.AddPersonPeriod(personPeriod);
			var assignment = PersonAssignmentFactory.CreateAssignmentWithMainShift(person, scenario, new DateTimePeriod(startDate, endDate));
			assignment.ShiftCategory.SetId(shiftCategoryId);
			
			var scheduleDay = new ProjectionChangedEventScheduleDay
			{
				ShiftCategoryId = Guid.Empty, // This category should not be used
				PersonPeriodId = personPeriodId,
				Date = startDate.Date
			};
			var cat = new AnalyticsShiftCategory { ShiftCategoryId = 1, ShiftCategoryCode = shiftCategoryId };
			var analyticsScenario = new AnalyticsScenario { ScenarioCode = scenarioId, ScenarioId = 66 };

			BusinessUnits.Add(businessUnit);
			Scenarios.Add(scenario);
			Persons.Add(person);
			Schedules.Add(assignment);

			AnalyticsScenarios.AddScenario(analyticsScenario);
			AnalyticsShiftCategories.AddShiftCategory(cat);
			AnalyticsPersonPeriods.AddPersonPeriod(new AnalyticsPersonPeriod { PersonId = 11, BusinessUnitId = 2, PersonPeriodCode = personPeriodId });
			IntervalLength.Has(15);

			Target.Handle(new ProjectionChangedEvent
			{
				ScheduleDays = new Collection<ProjectionChangedEventScheduleDay> { scheduleDay },
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
			var businessUnit = BusinessUnitFactory.CreateSimpleBusinessUnit("Default BU").WithId(businessUnitId);
			var scenario = ScenarioFactory.CreateScenario("Default", true, true).WithId(scenarioId);
			var person = PersonFactory.CreatePerson().WithId(personId);
			var personPeriod = PersonPeriodFactory.CreatePersonPeriod(new DateOnly(startDate)).WithId(personPeriodId);
			personPeriod.Team = TeamFactory.CreateTeam("Team", "Site");
			person.AddPersonPeriod(personPeriod);

			var analyticsScenario = new AnalyticsScenario { ScenarioCode = scenarioId, ScenarioId = 6 };
			var scheduleDay = new ProjectionChangedEventScheduleDay
			{
				Date = startDate.Date,
				NotScheduled = true
			};
			var analyticsPersonPeriod = new AnalyticsPersonPeriod { PersonId = 55, BusinessUnitId = 2, PersonPeriodCode = personPeriodId, PersonCode = personId};
			
			var existingSchedule = new FactScheduleRow
			{
				DatePart = new AnalyticsFactScheduleDate { ScheduleDateId = AnalyticsDates.Date(scheduleDay.Date).DateId },
				PersonPart = new AnalyticsFactSchedulePerson { BusinessUnitId = analyticsPersonPeriod.BusinessUnitId, PersonId = analyticsPersonPeriod.PersonId },
				TimePart = new AnalyticsFactScheduleTime { ScenarioId = analyticsScenario.ScenarioId }
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

			AnalyticsPersonPeriods.AddPersonPeriod(analyticsPersonPeriod);
			AnalyticsScenarios.AddScenario(analyticsScenario);
			
			AnalyticsSchedules.PersistFactScheduleDayCountRow(existingScheduleDayCount);
			AnalyticsSchedules.PersistFactScheduleBatch(new List<IFactScheduleRow> { existingSchedule });

			Target.Handle(new ProjectionChangedEvent
			{
				ScheduleDays = new Collection<ProjectionChangedEventScheduleDay> { scheduleDay },
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
			var businessUnit = BusinessUnitFactory.CreateSimpleBusinessUnit("Default BU").WithId(businessUnitId);
			var scenario = ScenarioFactory.CreateScenario("Default", true, true).WithId(scenarioId);
			var person = PersonFactory.CreatePerson().WithId(personId);
			var personPeriod = PersonPeriodFactory.CreatePersonPeriod(new DateOnly(startDate)).WithId(personPeriodId);
			personPeriod.Team = TeamFactory.CreateTeam("Team", "Site");
			person.AddPersonPeriod(personPeriod);
			var dayOffTemplate = DayOffFactory.CreateDayOff(new Description("Slackday", "SD"));
			var assignment = PersonAssignmentFactory.CreateAssignmentWithDayOff(person, scenario, new DateOnly(startDate), dayOffTemplate);

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
			var analyticsScenario = new AnalyticsScenario { ScenarioCode = scenarioId, ScenarioId = 66 };

			BusinessUnits.Add(businessUnit);
			Scenarios.Add(scenario);
			Persons.Add(person);
			Schedules.Add(assignment);

			AnalyticsScenarios.AddScenario(analyticsScenario);
			AnalyticsPersonPeriods.AddPersonPeriod(new AnalyticsPersonPeriod { PersonId = 11, BusinessUnitId = 2, PersonPeriodCode = personPeriodId });
			IntervalLength.Has(15);

			Target.Handle(new ProjectionChangedEvent
			{
				ScheduleDays = new Collection<ProjectionChangedEventScheduleDay> { scheduleDay },
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
			AnalyticsSchedules.FactScheduleDayCountRows.Single().DayOffShortName.Should().Be.EqualTo(dayOffTemplate.Description.ShortName);
		}

		[Test]
		public void ShouldPersistScheduleDayCountWhenFullDayAbsence()
		{
			var businessUnit = BusinessUnitFactory.CreateSimpleBusinessUnit("Default BU").WithId(businessUnitId);
			var scenario = ScenarioFactory.CreateScenario("Default", true, true).WithId(scenarioId);
			var person = PersonFactory.CreatePerson().WithId(personId);
			var personPeriod = PersonPeriodFactory.CreatePersonPeriod(new DateOnly(startDate)).WithId(personPeriodId);
			personPeriod.Team = TeamFactory.CreateTeam("Team", "Site");
			person.AddPersonPeriod(personPeriod);

			var absence = AbsenceFactory.CreateAbsence("FullDayAbsence").WithId(absenceId);
			var personAbsence = PersonAbsenceFactory.CreatePersonAbsence(person, scenario, new DateTimePeriod(startDate, endDate), absence);

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
			var analyticsScenario = new AnalyticsScenario { ScenarioCode = scenarioId, ScenarioId = 66 };
			var analyticsAbsence = new AnalyticsAbsence {AbsenceId = 123, AbsenceCode = absenceId};

			BusinessUnits.Add(businessUnit);
			Scenarios.Add(scenario);
			Persons.Add(person);
			Schedules.Add(personAbsence);

			AnalyticsAbsences.AddAbsence(analyticsAbsence);
			AnalyticsScenarios.AddScenario(analyticsScenario);
			AnalyticsPersonPeriods.AddPersonPeriod(new AnalyticsPersonPeriod { PersonId = 11, BusinessUnitId = 2, PersonPeriodCode = personPeriodId });
			IntervalLength.Has(15);

			Target.Handle(new ProjectionChangedEvent
			{
				ScheduleDays = new Collection<ProjectionChangedEventScheduleDay> { scheduleDay },
				ScenarioId = scenarioId,
				PersonId = personId,
				LogOnBusinessUnitId = businessUnitId
			});

			AnalyticsSchedules.FactScheduleRows.Count.Should().Be.EqualTo(8 * 4); // 8 hours of activity and 15 minute intervals (full day = 8 hours even though its actually saved as 9)
			AnalyticsSchedules.FactScheduleRows.ForEach(row => row.PersonPart.PersonId.Should().Be.EqualTo(11));
			AnalyticsSchedules.FactScheduleRows.ForEach(row => row.PersonPart.BusinessUnitId.Should().Be.EqualTo(2));
			AnalyticsSchedules.FactScheduleRows.ForEach(row => row.TimePart.ScenarioId.Should().Be.EqualTo(66));
			AnalyticsSchedules.FactScheduleRows.ForEach(row => row.TimePart.ShiftCategoryId.Should().Be.EqualTo(-1));
			AnalyticsSchedules.FactScheduleRows.ForEach(row => row.TimePart.AbsenceId.Should().Be.EqualTo(analyticsAbsence.AbsenceId));

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
			var businessUnit = BusinessUnitFactory.CreateSimpleBusinessUnit("Default BU").WithId(businessUnitId);
			var scenario = ScenarioFactory.CreateScenario("Default", true, true).WithId(scenarioId);
			var person = PersonFactory.CreatePerson().WithId(personId);
			var personPeriod = PersonPeriodFactory.CreatePersonPeriod(new DateOnly(startDate)).WithId(personPeriodId);
			personPeriod.Team = TeamFactory.CreateTeam("Team", "Site");
			person.AddPersonPeriod(personPeriod);
			var dayOffTemplate = DayOffFactory.CreateDayOff(new Description("Slackday", "SD"));
			var assignment = PersonAssignmentFactory.CreateAssignmentWithDayOff(person, scenario, new DateOnly(startDate), dayOffTemplate);

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

			AnalyticsPersonPeriods.AddPersonPeriod(new AnalyticsPersonPeriod { PersonId = 11, BusinessUnitId = 2, PersonPeriodCode = personPeriodId });
			IntervalLength.Has(15);

			Assert.Throws<ScenarioMissingInAnalyticsException>(() => Target.Handle(new ProjectionChangedEvent
			{
				ScheduleDays = new Collection<ProjectionChangedEventScheduleDay> { scheduleDay },
				ScenarioId = scenarioId,
				PersonId = personId,
				LogOnBusinessUnitId = businessUnitId
			}));
		}
		
		[Test]
		[Toggle(Toggles.ETL_SpeedUpFactScheduleNightly_38019)]
		public void ShouldNotBeHandledBecauseFilterWhenNotDefaultScenarioAndNotReportable()
		{
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
				ScheduleDays = new Collection<ProjectionChangedEventScheduleDay> { scheduleDay },
				ScenarioId = scenarioId,
				IsDefaultScenario = false,
				LogOnBusinessUnitId = businessUnitId
			}));
		}
	}
}