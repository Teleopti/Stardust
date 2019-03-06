using System;
using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.ApplicationLayer;
using Teleopti.Ccc.Domain.ApplicationLayer.Commands;
using Teleopti.Ccc.Domain.ApplicationLayer.Events;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Common.Time;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.SaveSchedulePart;
using Teleopti.Ccc.Domain.Staffing;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.TestCommon.FakeRepositories;
using Teleopti.Ccc.TestCommon.IoC;


namespace Teleopti.Ccc.DomainTest.ApplicationLayer
{
	[DomainTest]
	[NoDefaultData]
	public class AddIntradayAbsenceCommandHandlerTest : IIsolateSystem
	{
		public AddIntradayAbsenceCommandHandler Target;
		public IScheduleStorage ScheduleStorage;
		public FakeScenarioRepository ScenarioRepository;
		public FakePersonRepository PersonRepository;
		public FakeAbsenceRepository AbsenceRepository;
		public FakePersonAssignmentRepository PersonAssignmentRepository;
		public FakePersonAbsenceRepository PersonAbsenceRepository;
		public MutableNow Now;
		public FakeUserTimeZone UserTimeZone;

		public void Isolate(IIsolate isolate)
		{
			isolate.UseTestDouble<ScheduleDayDifferenceSaver>().For<IScheduleDayDifferenceSaver>();
			isolate.UseTestDouble<FakePersonRepository>().For<IProxyForId<IPerson>>();
			isolate.UseTestDouble<FakeAbsenceRepository>().For<IProxyForId<IAbsence>>();
			isolate.UseTestDouble<AddIntradayAbsenceCommandHandler>().For<IHandleCommand<AddIntradayAbsenceCommand>>();
		}

		[Test]
		public void ShouldRaiseIntradayAbsenceAddedEvent()
		{
			var scenario = ScenarioRepository.Has("Default");
			var person = new Person().WithId();
			PersonRepository.Has(person);
			var absence = new Absence().WithId();
			AbsenceRepository.Has(absence);

			var operatedPersonId = Guid.NewGuid();
			var command = new AddIntradayAbsenceCommand
				{
					AbsenceId = absence.Id.Value,
					PersonId = person.Id.Value,
					StartTime = new DateTime(2013, 11, 27, 14, 00, 00, DateTimeKind.Utc),
					EndTime = new DateTime(2013, 11, 27, 15, 00, 00, DateTimeKind.Utc),
					TrackedCommandInfo = new TrackedCommandInfo
					{
						OperatedPersonId = operatedPersonId
					}
				};
			Target.Handle(command);

			var personAbsence = PersonAbsenceRepository.LoadAll().Single() as PersonAbsence;
			var @event = personAbsence.PopAllEvents(null).Single() as PersonAbsenceAddedEvent;
			@event.AbsenceId.Should().Be(absence.Id.Value);
			@event.PersonId.Should().Be(person.Id.Value);
			@event.ScenarioId.Should().Be(scenario.Id.Value);
			@event.StartDateTime.Should().Be(command.StartTime);
			@event.EndDateTime.Should().Be(command.EndTime);
			@event.InitiatorId.Should().Be(operatedPersonId);
			@event.LogOnBusinessUnitId.Should().Be(scenario.GetOrFillWithBusinessUnit_DONTUSE().Id.GetValueOrDefault());
		}

		[Test]
		public void ShouldSetupEntityState()
		{
			ScenarioRepository.Has("Default");
			var person = new Person().WithId();
			PersonRepository.Has(person);
			var absence = new Absence().WithId();
			AbsenceRepository.Has(absence);

			var command = new AddIntradayAbsenceCommand
			{
				AbsenceId = absence.Id.Value,
				PersonId = person.Id.Value,
				StartTime = new DateTime(2013, 11, 27, 14, 00, 00, DateTimeKind.Utc),
					EndTime = new DateTime(2013, 11, 27, 15, 00, 00, DateTimeKind.Utc),
					TrackedCommandInfo = new TrackedCommandInfo()
				};
			Target.Handle(command);

			var personAbsence = PersonAbsenceRepository.LoadAll().Single() as PersonAbsence;
			var absenceLayer = personAbsence.Layer as AbsenceLayer;
			personAbsence.Person.Should().Be(person);
			absenceLayer.Payload.Should().Be(absence);
			absenceLayer.Period.StartDateTime.Should().Be(command.StartTime);
			absenceLayer.Period.EndDateTime.Should().Be(command.EndTime);
		}

		[Test]
		public void ShouldNotChangeShiftEndAfterAddingAbsence()
		{
			var person = new Person().WithId();
			PersonRepository.Has(person);
			var absence = new Absence().WithId();
			AbsenceRepository.Has(absence);
			var mainActivity = ActivityFactory.CreateActivity("Phone");
			var shiftCategory = ShiftCategoryFactory.CreateShiftCategory();
			var scenario = ScenarioRepository.Has("Default");
			var pa = PersonAssignmentFactory.CreateAssignmentWithMainShift(person, scenario, mainActivity, new DateTimePeriod(2013, 11, 27, 8, 2013, 11, 27, 16), shiftCategory);
			PersonAssignmentRepository.Add(pa);
			
			var operatedPersonId = Guid.NewGuid();
			var command = new AddIntradayAbsenceCommand
			{
				AbsenceId = absence.Id.Value,
				PersonId = person.Id.Value,
				StartTime = new DateTime(2013, 11, 27, 14, 00, 00, DateTimeKind.Utc),
				EndTime = new DateTime(2013, 11, 27, 18, 00, 00, DateTimeKind.Utc),
				TrackedCommandInfo = new TrackedCommandInfo
				{
					OperatedPersonId = operatedPersonId
				}
			};
			Target.Handle(command);

			var scheduleDay = getScheduleDay(new DateOnly(2013, 11, 27), person);
			var projection = scheduleDay.ProjectionService().CreateProjection().ToList();

			projection.Last().Period.StartDateTime.Should().Be(new DateTime(2013, 11, 27, 14, 00, 00, DateTimeKind.Utc));
			projection.Last().Period.EndDateTime.Should().Be(new DateTime(2013, 11, 27, 16, 00, 00, DateTimeKind.Utc));
			projection.Last().Payload.Id.Should().Be(absence.Id.Value);
		}

		[Test]
		public void ShouldConvertTimesFromUsersTimeZone()
		{
			ScenarioRepository.Has("Default");
			var userTimeZone = TimeZoneInfoFactory.HawaiiTimeZoneInfo();
			UserTimeZone.Is(userTimeZone);
			var person = new Person().WithId();
			PersonRepository.Has(person);
			var absence = new Absence().WithId();
			AbsenceRepository.Has(absence);

			var command = new AddIntradayAbsenceCommand
			{
				AbsenceId = absence.Id.Value,
				PersonId = person.Id.Value,
				StartTime = new DateTime(2013, 11, 27, 14, 00, 00),
					EndTime = new DateTime(2013, 11, 27, 15, 00, 00),
					TrackedCommandInfo = new TrackedCommandInfo()
				};
			Target.Handle(command);

			var personAbsence = PersonAbsenceRepository.LoadAll().Single() as PersonAbsence;
			var absenceLayer = personAbsence.Layer as AbsenceLayer;
			absenceLayer.Period.StartDateTime.Should().Be(TimeZoneInfo.ConvertTimeToUtc(command.StartTime, userTimeZone));
			absenceLayer.Period.EndDateTime.Should().Be(TimeZoneInfo.ConvertTimeToUtc(command.EndTime, userTimeZone));
			var @event = personAbsence.PopAllEvents(null).Single() as PersonAbsenceAddedEvent;
			@event.StartDateTime.Should().Be(TimeZoneInfo.ConvertTimeToUtc(command.StartTime, userTimeZone));
			@event.EndDateTime.Should().Be(TimeZoneInfo.ConvertTimeToUtc(command.EndTime, userTimeZone));
		}

		private IScheduleDictionary getScheduleDictionary(DateOnly date, IPerson person)
		{
			var period = date.ToDateOnlyPeriod().Inflate(1);
			var schedules = ScheduleStorage.FindSchedulesForPersonOnlyInGivenPeriod(person,
				new ScheduleDictionaryLoadOptions(false, false),
				period,
				ScenarioRepository.LoadDefaultScenario());
			return schedules;
		}

		private IScheduleDay getScheduleDay(DateOnly date, IPerson person)
		{
			var schedules = getScheduleDictionary(date, person);
			return schedules[person].ScheduledDay(date);
		}
	}
}