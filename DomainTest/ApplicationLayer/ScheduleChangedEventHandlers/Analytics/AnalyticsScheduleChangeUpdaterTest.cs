using System;
using System.Collections.Generic;
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
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure.Analytics;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.Assignment;
using Teleopti.Ccc.Domain.Scheduling.TimeLayer;
using Teleopti.Ccc.IocCommon;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.TestCommon.FakeRepositories;
using Teleopti.Ccc.TestCommon.IoC;


namespace Teleopti.Ccc.DomainTest.ApplicationLayer.ScheduleChangedEventHandlers.Analytics
{
	[DomainTest]
	public class AnalyticsScheduleChangeUpdaterTest : IIsolateSystem, IExtendSystem
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
		public FakeAnalyticsOvertimeRepository AnalyticsOvertimes;
		public FakeAnalyticsDayOffRepository AnalyticsDayOffRepository;

		private readonly Guid businessUnitId = Guid.NewGuid();
		private readonly Guid scenarioId = Guid.NewGuid();
		private readonly Guid personId = Guid.NewGuid();
		private readonly Guid personPeriodId = Guid.NewGuid();
		private readonly Guid absenceId = Guid.NewGuid();
		private readonly DateTime startDate = new DateTime(2017, 03, 07, 8, 0, 0, DateTimeKind.Utc);
		private readonly DateTime endDate = new DateTime(2017, 03, 07, 17, 0, 0, DateTimeKind.Utc);

		public void Extend(IExtend extend, IocConfiguration configuration)
		{
			extend.AddService<AnalyticsScheduleChangeUpdater>();
		}
		
		public void Isolate(IIsolate isolate)
		{
			var fetcher = new FakeIntervalLengthFetcher();
			fetcher.Has(15);
			isolate.UseTestDouble(fetcher).For<IIntervalLengthFetcher>();
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

			var analyticsPersonPeriod = new AnalyticsPersonPeriod
			{
				PersonId = 1,
				BusinessUnitId = 2,
				PersonPeriodCode = personPeriodId
			};
			var existingSchedule = new FactScheduleRow
			{
				DatePart = new AnalyticsFactScheduleDate
				{
					ScheduleDateId = AnalyticsDates.Date(startDate.Date).DateId
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

			Target.Handle(new ScheduleChangedEvent
			{
				StartDateTime = startDate,
				EndDateTime = endDate,
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
			
			BusinessUnits.Add(businessUnit);
			Scenarios.Add(scenario);
			Persons.Add(person);
			Schedules.Add(assignment);

			AnalyticsScenarios.AddScenario(analyticsScenario);

			Assert.Throws<PersonPeriodMissingInAnalyticsException>(() => Target.Handle(new ScheduleChangedEvent
			{
				StartDateTime = startDate,
				EndDateTime = endDate,
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

			Target.Handle(new ScheduleChangedEvent
			{
				StartDateTime = startDate,
				EndDateTime = endDate,
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

			Target.Handle(new ScheduleChangedEvent
			{
				StartDateTime = startDate,
				EndDateTime = endDate,
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

			Target.Handle(new ScheduleChangedEvent
			{
				StartDateTime = startDate,
				EndDateTime = endDate,
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

			Target.Handle(new ScheduleChangedEvent
			{
				StartDateTime = startDate,
				EndDateTime = endDate,
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

			Target.Handle(new ScheduleChangedEvent
			{
				StartDateTime = startDate,
				EndDateTime = endDate,
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

			Assert.Throws<ScenarioMissingInAnalyticsException>(() => Target.Handle(new ScheduleChangedEvent
			{
				StartDateTime = startDate,
				EndDateTime = endDate,
				ScenarioId = scenarioId,
				PersonId = personId,
				LogOnBusinessUnitId = businessUnitId
			}));
		}

		[Test]
		public void ShouldNotBeHandledBecauseFilterWhenNotDefaultScenarioAndNotReportable()
		{
			AnalyticsDates.HasDatesBetween(new DateTime(1999, 12, 31), new DateTime(2030, 12, 31));
			var businessUnit = BusinessUnitFactory.CreateSimpleBusinessUnit("Default BU").WithId(businessUnitId);
			var scenario = ScenarioFactory.CreateScenario("Default", false, false).WithId(scenarioId);
			
			BusinessUnits.Add(businessUnit);
			Scenarios.Add(scenario);

			Assert.DoesNotThrow(() => Target.Handle(new ScheduleChangedEvent
			{
				StartDateTime = startDate,
				EndDateTime = endDate,
				ScenarioId = scenarioId,
				LogOnBusinessUnitId = businessUnitId
			}));
		}

		[Test]
		public void ShouldNotThrowWhenScenarioDoesNotExistInApp()
		{
			AnalyticsDates.HasDatesBetween(new DateTime(1999, 12, 31), new DateTime(2030, 12, 31));
			var businessUnit = BusinessUnitFactory.CreateSimpleBusinessUnit("Default BU").WithId(businessUnitId);
			var scenario = ScenarioFactory.CreateScenario("Default", false, false).WithId(scenarioId);

			BusinessUnits.Add(businessUnit);
			Scenarios.Add(scenario);

			Assert.DoesNotThrow(() => Target.Handle(new ScheduleChangedEvent
			{
				StartDateTime = startDate,
				EndDateTime = endDate,
				ScenarioId = Guid.NewGuid(),
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
			
			AnalyticsScenarios.AddScenario(analyticsScenario);
			AnalyticsPersonPeriods.AddOrUpdatePersonPeriod(new AnalyticsPersonPeriod
			{
				PersonId = 11,
				BusinessUnitId = 2,
				PersonPeriodCode = personPeriodId
			});
			IntervalLength.Has(intervalLengthInMinute);

			// 2017-12-31 is out of analytics date range, schedule change for it will not be processed
			Target.Handle(new ScheduleChangedEvent
			{
				StartDateTime = scheduleStartDate.AddDays(-1),
				EndDateTime = scheduleStartDate.AddDays(1).AddHours(4).AddMinutes(45),
				ScenarioId = scenarioId,
				PersonId = personId,
				LogOnBusinessUnitId = businessUnitId
			});

			var factScheduleDays = AnalyticsSchedules.FactScheduleDayCountRows;
			factScheduleDays.Count.Should().Be.EqualTo(2);
			factScheduleDays.First().StartTime.Should().Be.EqualTo(assignment2Period.StartDateTime);
			// Full DayOff, start from 00:00
			factScheduleDays.Second().StartTime.Should().Be.EqualTo(assignmentDayOff.Date.Date);

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
			
			AnalyticsScenarios.AddScenario(analyticsScenario);
			AnalyticsPersonPeriods.AddOrUpdatePersonPeriod(new AnalyticsPersonPeriod
			{
				PersonId = 11,
				BusinessUnitId = 2,
				PersonPeriodCode = personPeriodId
			});
			IntervalLength.Has(intervalLengthInMinute);

			// No assignment for 2017-03-06, so schedule will be deleted for that day
			Target.Handle(new ScheduleChangedEvent
			{
				StartDateTime = startDate.AddDays(-1),
				EndDateTime = startDate.AddDays(1).AddHours(4).AddMinutes(45),
				ScenarioId = scenarioId,
				PersonId = personId,
				LogOnBusinessUnitId = businessUnitId
			});

			var factScheduleDays = AnalyticsSchedules.FactScheduleDayCountRows;
			factScheduleDays.Count.Should().Be.EqualTo(2);
			factScheduleDays.First().StartTime.Should().Be.EqualTo(assignmentPeriod.StartDateTime);
			// Full DayOff, start from 00:00
			factScheduleDays.Second().StartTime.Should().Be.EqualTo(assignmentDayOff.Date.Date);

			AnalyticsDayOffRepository.DayOffs().Count.Should().Be.EqualTo(1);
			AnalyticsSchedules.FactScheduleRows.Count.Should().Be
				.EqualTo((int)assignmentPeriod.ElapsedTime().TotalMinutes / intervalLengthInMinute);

			foreach (var row in AnalyticsSchedules.FactScheduleRows)
			{
				row.DatePart.ShiftStartTime.Should().Be.EqualTo(startDate);
			}
		}

		[Test]
		public void ShouldSaveScheduleForOvertimeActivityWhenActivityStartTimeIsDifferentWithBelongsToDate()
		{
			AnalyticsDates.HasDatesBetween(new DateTime(2017, 3, 6), new DateTime(2030, 12, 31));
			var businessUnit = BusinessUnitFactory.CreateSimpleBusinessUnit("Default BU").WithId(businessUnitId);
			var scenario = ScenarioFactory.CreateScenario("Default", true, true).WithId(scenarioId);
			var person = PersonFactory.CreatePerson().WithId(personId);
			var personPeriod = PersonPeriodFactory.CreatePersonPeriod(new DateOnly(2017, 3, 6)).WithId(personPeriodId);
			personPeriod.Team = TeamFactory.CreateTeam("Team", "Site");
			person.AddPersonPeriod(personPeriod);
			var assignment1 = PersonAssignmentFactory.CreateAssignmentWithMainShift(person, scenario,
				new DateTimePeriod(new DateTime(2017, 03, 07, 08, 00, 00, DateTimeKind.Utc), new DateTime(2017, 03, 07, 18, 00, 00, DateTimeKind.Utc)));
			var assignment2 = PersonAssignmentFactory.CreateAssignmentWithMainShift(person, scenario,
				new DateTimePeriod(new DateTime(2017, 03, 06, 20, 00, 00, DateTimeKind.Utc), new DateTime(2017, 03, 07, 03, 00, 0, DateTimeKind.Utc)));

			var overtimeActivity = new Activity("test").WithId();
			var overtimePeriod = new DateTimePeriod(new DateTime(2017, 03, 07, 3, 00, 00, DateTimeKind.Utc),
				new DateTime(2017, 03, 07, 04, 00, 0, DateTimeKind.Utc));
			var multiplicatorDefinitionSet = new MultiplicatorDefinitionSet("test", MultiplicatorType.Overtime).WithId();
			assignment2.AddOvertimeActivity(overtimeActivity, overtimePeriod, multiplicatorDefinitionSet);
			
			var analyticsScenario = new AnalyticsScenario
			{
				ScenarioCode = scenarioId,
				ScenarioId = 3
			};

			var analyticsPersonPeriod = new AnalyticsPersonPeriod
			{
				PersonId = 1,
				BusinessUnitId = 2,
				PersonPeriodCode = personPeriodId
			};

			BusinessUnits.Add(businessUnit);
			Scenarios.Add(scenario);
			Persons.Add(person);
			Schedules.Add(assignment1);
			Schedules.Add(assignment2);

			AnalyticsScenarios.AddScenario(analyticsScenario);
			AnalyticsPersonPeriods.AddOrUpdatePersonPeriod(analyticsPersonPeriod);
			AnalyticsOvertimes.AddOrUpdate(new AnalyticsOvertime
			{
				OvertimeId = 2,
				OvertimeName = "test",
				OvertimeCode = multiplicatorDefinitionSet.Id.Value
			});

			var belongsToDate = new DateTime(2017, 03, 06, 0, 0, 0, DateTimeKind.Utc);
			Target.Handle(new ScheduleChangedEvent
			{
				StartDateTime = overtimePeriod.StartDateTime,
				EndDateTime = overtimePeriod.EndDateTime,
				ScenarioId = scenarioId,
				LogOnBusinessUnitId = businessUnitId,
				PersonId = personId,
				Date = belongsToDate
			});

			var schedules =
				AnalyticsSchedules.FactScheduleRows.Where(a => a.DatePart.ShiftStartTime.Date.Equals(belongsToDate.Date)).ToList();
			schedules.Any().Should().Be(true);
			schedules.Any(a => a.TimePart.OverTimeId > 0).Should().Be(true);
		}

		[Test]
		public void ShouldSaveScheduleWithDuplicateBelongsToDate()
		{
			AnalyticsDates.HasDatesBetween(new DateTime(2017, 3, 6), new DateTime(2030, 12, 31));
			var businessUnit = BusinessUnitFactory.CreateSimpleBusinessUnit("Default BU").WithId(businessUnitId);
			var scenario = ScenarioFactory.CreateScenario("Default", true, true).WithId(scenarioId);
			var person = PersonFactory.CreatePerson().WithId(personId);
			var personPeriod = PersonPeriodFactory.CreatePersonPeriod(new DateOnly(2017, 3, 6)).WithId(personPeriodId);
			personPeriod.Team = TeamFactory.CreateTeam("Team", "Site");
			person.AddPersonPeriod(personPeriod);
			var assignment1 = PersonAssignmentFactory.CreateAssignmentWithMainShift(person, scenario,
				new DateTimePeriod(new DateTime(2017, 03, 07, 08, 00, 00, DateTimeKind.Utc), new DateTime(2017, 03, 07, 18, 00, 00, DateTimeKind.Utc)));

			var analyticsScenario = new AnalyticsScenario
			{
				ScenarioCode = scenarioId,
				ScenarioId = 3
			};

			var analyticsPersonPeriod = new AnalyticsPersonPeriod
			{
				PersonId = 1,
				BusinessUnitId = 2,
				PersonPeriodCode = personPeriodId
			};

			BusinessUnits.Add(businessUnit);
			Scenarios.Add(scenario);
			Persons.Add(person);
			Schedules.Add(assignment1);

			AnalyticsScenarios.AddScenario(analyticsScenario);
			AnalyticsPersonPeriods.AddOrUpdatePersonPeriod(analyticsPersonPeriod);
			
			var belongsToDate = new DateTime(2017, 03, 07, 1, 0, 0, DateTimeKind.Utc);
			Target.Handle(new ScheduleChangedEvent
			{
				StartDateTime = assignment1.Period.StartDateTime,
				EndDateTime = assignment1.Period.EndDateTime,
				ScenarioId = scenarioId,
				LogOnBusinessUnitId = businessUnitId,
				PersonId = personId,
				Date = belongsToDate
			});

			var schedules =
				AnalyticsSchedules.FactScheduleRows.Where(a => a.DatePart.ShiftStartTime.Date.Equals(belongsToDate.Date)).ToList();
			schedules.Any().Should().Be(true);
		}

		[Test]
		public void ShouldeRemoveScheduleForOvertimeActivityWhenActivityStartTimeIsDifferentWithBelongsToDate()
		{
			AnalyticsDates.HasDatesBetween(new DateTime(2017, 3, 6), new DateTime(2030, 12, 31));
			var businessUnit = BusinessUnitFactory.CreateSimpleBusinessUnit("Default BU").WithId(businessUnitId);
			var scenario = ScenarioFactory.CreateScenario("Default", true, true).WithId(scenarioId);
			var person = PersonFactory.CreatePerson().WithId(personId);
			var personPeriod = PersonPeriodFactory.CreatePersonPeriod(new DateOnly(2017, 3, 6)).WithId(personPeriodId);
			personPeriod.Team = TeamFactory.CreateTeam("Team", "Site");
			person.AddPersonPeriod(personPeriod);
			var assignment1 = PersonAssignmentFactory.CreateAssignmentWithMainShift(person, scenario,
				new DateTimePeriod(new DateTime(2017, 03, 07, 08, 00, 00, DateTimeKind.Utc), new DateTime(2017, 03, 07, 18, 00, 00, DateTimeKind.Utc)));
			var assignment2 = PersonAssignmentFactory.CreateAssignmentWithMainShift(person, scenario,
				new DateTimePeriod(new DateTime(2017, 03, 06, 20, 00, 00, DateTimeKind.Utc), new DateTime(2017, 03, 07, 03, 00, 0, DateTimeKind.Utc)));

			var overtimeActivity = new Activity("test").WithId();
			var overtimePeriod = new DateTimePeriod(new DateTime(2017, 03, 07, 3, 00, 00, DateTimeKind.Utc),
				new DateTime(2017, 03, 07, 04, 00, 0, DateTimeKind.Utc));
			var multiplicatorDefinitionSet = new MultiplicatorDefinitionSet("test", MultiplicatorType.Overtime).WithId();
			assignment2.AddOvertimeActivity(overtimeActivity, overtimePeriod, multiplicatorDefinitionSet);

			var analyticsScenario = new AnalyticsScenario
			{
				ScenarioCode = scenarioId,
				ScenarioId = 3
			};

			var analyticsPersonPeriod = new AnalyticsPersonPeriod
			{
				PersonId = 1,
				BusinessUnitId = 2,
				PersonPeriodCode = personPeriodId,
				PersonCode = personId
			};

			BusinessUnits.Add(businessUnit);
			Scenarios.Add(scenario);
			Persons.Add(person);
			Schedules.Add(assignment1);
			Schedules.Add(assignment2);

			AnalyticsScenarios.AddScenario(analyticsScenario);
			AnalyticsPersonPeriods.AddOrUpdatePersonPeriod(analyticsPersonPeriod);
			AnalyticsOvertimes.AddOrUpdate(new AnalyticsOvertime
			{
				OvertimeId = 2,
				OvertimeName = "test",
				OvertimeCode = multiplicatorDefinitionSet.Id.Value
			});

			var belongsToDate = new DateTime(2017, 03, 06, 0, 0, 0, DateTimeKind.Utc);
			Target.Handle(new ScheduleChangedEvent
			{
				StartDateTime = overtimePeriod.StartDateTime,
				EndDateTime = overtimePeriod.EndDateTime,
				ScenarioId = scenarioId,
				LogOnBusinessUnitId = businessUnitId,
				PersonId = personId,
				Date = belongsToDate
			});

			var overtimeShiftLayer = assignment2.ShiftLayers.OfType<OvertimeShiftLayer>().FirstOrDefault();
			assignment2.RemoveActivity(overtimeShiftLayer);
			Target.Handle(new ScheduleChangedEvent
			{
				StartDateTime = overtimePeriod.StartDateTime,
				EndDateTime = overtimePeriod.EndDateTime,
				ScenarioId = scenarioId,
				LogOnBusinessUnitId = businessUnitId,
				PersonId = personId,
				Date = belongsToDate
			});

			var schedules =
				AnalyticsSchedules.FactScheduleRows.Where(a => a.DatePart.ScheduleDateId == 0).ToList();
			schedules.Any().Should().Be(true);
			schedules.Any(a => a.TimePart.OverTimeId > 0).Should().Be(false);
		}
	}
}