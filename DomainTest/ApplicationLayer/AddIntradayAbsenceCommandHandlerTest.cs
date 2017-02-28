﻿using System;
using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.ApplicationLayer.Commands;
using Teleopti.Ccc.Domain.ApplicationLayer.Events;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.SaveSchedulePart;
using Teleopti.Ccc.Domain.Security.Authentication;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.TestCommon.FakeRepositories;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.ApplicationLayer
{
	[TestFixture]
	[TestWithStaticDependenciesAvoidUse]
	public class AddIntradayAbsenceCommandHandlerTest
	{

		private FakeWriteSideRepository<IPerson> _personRepository;
		private FakeWriteSideRepository<IAbsence> _absenceRepository;
		private FakeCurrentScenario _currentScenario;
		private FakeScheduleStorage _scheduleStorage;
		private PersonAbsenceCreator _personAbsenceCreator;
		private IAbsenceCommandConverter _absenceCommandConverter;

		[SetUp]
		public void Setup()
		{
			var personAbsenceAccountRepository = new FakePersonAbsenceAccountRepository();
			_personRepository = new FakeWriteSideRepository<IPerson> { PersonFactory.CreatePersonWithId() };
			_absenceRepository = new FakeWriteSideRepository<IAbsence> { AbsenceFactory.CreateAbsenceWithId() };
			_currentScenario = new FakeCurrentScenario();
			
			_scheduleStorage = new FakeScheduleStorage();
			var scheduleDifferenceSaver = new FakeScheduleDifferenceSaver(_scheduleStorage);
			
			var businessRulesForAccountUpdate = new BusinessRulesForPersonalAccountUpdate(personAbsenceAccountRepository, new SchedulingResultStateHolder());
			var saveSchedulePartService = new SaveSchedulePartService(scheduleDifferenceSaver, personAbsenceAccountRepository, new DoNothingScheduleDayChangeCallBack());
			_personAbsenceCreator = new PersonAbsenceCreator (saveSchedulePartService, businessRulesForAccountUpdate );

			_absenceCommandConverter = new AbsenceCommandConverter(_currentScenario, _personRepository, _absenceRepository, _scheduleStorage, new UtcTimeZone());
		}



		[Test]
		public void ShouldRaiseIntradayAbsenceAddedEvent()
		{
			var target = new AddIntradayAbsenceCommandHandler(_personAbsenceCreator, _absenceCommandConverter);

			var operatedPersonId = Guid.NewGuid();
			var command = new AddIntradayAbsenceCommand
				{
					AbsenceId = _absenceRepository.Single().Id.Value,
					PersonId = _personRepository.Single().Id.Value,
					StartTime = new DateTime(2013, 11, 27, 14, 00, 00, DateTimeKind.Utc),
					EndTime = new DateTime(2013, 11, 27, 15, 00, 00, DateTimeKind.Utc),
					TrackedCommandInfo = new TrackedCommandInfo
					{
						OperatedPersonId = operatedPersonId
					}
				};
			target.Handle(command);

			var personAbsence = _scheduleStorage.LoadAll().Single() as PersonAbsence;
			var @event = personAbsence.PopAllEvents().Single() as PersonAbsenceAddedEvent;
			@event.AbsenceId.Should().Be(_absenceRepository.Single().Id.Value);
			@event.PersonId.Should().Be(_personRepository.Single().Id.Value);
			@event.ScenarioId.Should().Be(_currentScenario.Current().Id.Value);
			@event.StartDateTime.Should().Be(command.StartTime);
			@event.EndDateTime.Should().Be(command.EndTime);
			@event.InitiatorId.Should().Be(operatedPersonId);
			@event.LogOnBusinessUnitId.Should().Be(_currentScenario.Current().BusinessUnit.Id.GetValueOrDefault());
		}

		[Test]
		public void ShouldSetupEntityState()
		{
			var target = new AddIntradayAbsenceCommandHandler(_personAbsenceCreator, _absenceCommandConverter);

			var command = new AddIntradayAbsenceCommand
				{
					AbsenceId = _absenceRepository.Single().Id.Value,
					PersonId = _personRepository.Single().Id.Value,
					StartTime = new DateTime(2013, 11, 27, 14, 00, 00, DateTimeKind.Utc),
					EndTime = new DateTime(2013, 11, 27, 15, 00, 00, DateTimeKind.Utc),
					TrackedCommandInfo = new TrackedCommandInfo()
				};
			target.Handle(command);

			var personAbsence = _scheduleStorage.LoadAll().Single() as PersonAbsence;
			var absenceLayer = personAbsence.Layer as AbsenceLayer;
			personAbsence.Person.Should().Be(_personRepository.Single());
			absenceLayer.Payload.Should().Be(_absenceRepository.Single());
			absenceLayer.Period.StartDateTime.Should().Be(command.StartTime);
			absenceLayer.Period.EndDateTime.Should().Be(command.EndTime);
		}

		[Test]
		public void ShouldNotChangeShiftEndAfterAddingAbsence()
		{
			var mainActivity = ActivityFactory.CreateActivity("Phone");
			var shiftCategory = ShiftCategoryFactory.CreateShiftCategory();
			var scenario = _currentScenario.Current();
			var pa = PersonAssignmentFactory.CreateAssignmentWithMainShift(_personRepository.Single(), scenario, mainActivity, new DateTimePeriod(2013, 11, 27, 8, 2013, 11, 27, 16), shiftCategory);
			_scheduleStorage.Add(pa);

			var target = new AddIntradayAbsenceCommandHandler(_personAbsenceCreator, _absenceCommandConverter);

			var operatedPersonId = Guid.NewGuid();
			var command = new AddIntradayAbsenceCommand
			{
				AbsenceId = _absenceRepository.Single().Id.Value,
				PersonId = _personRepository.Single().Id.Value,
				StartTime = new DateTime(2013, 11, 27, 14, 00, 00, DateTimeKind.Utc),
				EndTime = new DateTime(2013, 11, 27, 18, 00, 00, DateTimeKind.Utc),
				TrackedCommandInfo = new TrackedCommandInfo
				{
					OperatedPersonId = operatedPersonId
				}
			};
			target.Handle(command);

			var scheduleDay = getScheduleDay(new DateOnly(2013, 11, 27), _personRepository.Single());
			var projection = scheduleDay.ProjectionService().CreateProjection().ToList();

			projection.Last().Period.StartDateTime.Should().Be(new DateTime(2013, 11, 27, 14, 00, 00, DateTimeKind.Utc));
			projection.Last().Period.EndDateTime.Should().Be(new DateTime(2013, 11, 27, 16, 00, 00, DateTimeKind.Utc));
			projection.Last().Payload.Id.Should().Be(_absenceRepository.Single().Id.Value);
		}

		[Test]
		public void ShouldConvertTimesFromUsersTimeZone()
		{
			var userTimeZone = TimeZoneInfoFactory.HawaiiTimeZoneInfo();
			var absenceCommandConverter = new AbsenceCommandConverter(_currentScenario, _personRepository, _absenceRepository, _scheduleStorage, new SpecificTimeZone(userTimeZone));
			var target = new AddIntradayAbsenceCommandHandler(_personAbsenceCreator, absenceCommandConverter);

			var command = new AddIntradayAbsenceCommand
				{
					AbsenceId = _absenceRepository.Single().Id.Value,
					PersonId = _personRepository.Single().Id.Value,
					StartTime = new DateTime(2013, 11, 27, 14, 00, 00),
					EndTime = new DateTime(2013, 11, 27, 15, 00, 00),
					TrackedCommandInfo = new TrackedCommandInfo()
				};
			target.Handle(command);

			var personAbsence = _scheduleStorage.LoadAll().Single() as PersonAbsence;
			var absenceLayer = personAbsence.Layer as AbsenceLayer;
			absenceLayer.Period.StartDateTime.Should().Be(TimeZoneInfo.ConvertTimeToUtc(command.StartTime, userTimeZone));
			absenceLayer.Period.EndDateTime.Should().Be(TimeZoneInfo.ConvertTimeToUtc(command.EndTime, userTimeZone));
			var @event = personAbsence.PopAllEvents().Single() as PersonAbsenceAddedEvent;
			@event.StartDateTime.Should().Be(TimeZoneInfo.ConvertTimeToUtc(command.StartTime, userTimeZone));
			@event.EndDateTime.Should().Be(TimeZoneInfo.ConvertTimeToUtc(command.EndTime, userTimeZone));
		}

		private IScheduleDictionary getScheduleDictionary(DateOnly date, IPerson person)
		{
			var period = new DateOnlyPeriod(date, date).Inflate(1);
			var schedules = _scheduleStorage.FindSchedulesForPersonOnlyInGivenPeriod(person,
				new ScheduleDictionaryLoadOptions(false, false),
				period,
				_currentScenario.Current());
			return schedules;
		}

		private IScheduleDay getScheduleDay(DateOnly date, IPerson person)
		{
			var schedules = getScheduleDictionary(date, person);
			return schedules[person].ScheduledDay(date);
		}
	}
}