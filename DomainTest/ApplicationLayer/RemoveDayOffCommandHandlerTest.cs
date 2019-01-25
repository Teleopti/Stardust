using NUnit.Framework;
using SharpTestsEx;
using System;
using System.Linq;
using Teleopti.Ccc.Domain.ApplicationLayer;
using Teleopti.Ccc.Domain.ApplicationLayer.Commands;
using Teleopti.Ccc.Domain.ApplicationLayer.Events;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.PersonalAccount;
using Teleopti.Ccc.Domain.Tracking;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.TestCommon.FakeRepositories;
using Teleopti.Ccc.TestCommon.IoC;


namespace Teleopti.Ccc.DomainTest.ApplicationLayer
{
	[DomainTest]
	public class RemoveDayOffCommandHandlerTest : IIsolateSystem
	{
		public RemoveDayOffCommandHandler Target;
		public FakePersonRepository PersonRepository;
		public FakeScenarioRepository ScenarioRepository;
		public FakeDayOffTemplateRepository DayOffTemplateRepository;
		public FakePersonAssignmentRepository PersonAssignmentRepository;
		public FakeAbsenceRepository AbsenceRepository;
		public FakePersonAbsenceAccountRepository PersonAbsenceAccountRepository;
		public IScheduleStorage ScheduleStorage;
		public FakeLoggedOnUser LoggedOnUser;

		public void Isolate(IIsolate isolate)
		{
			isolate.UseTestDouble<FakeLoggedOnUser>().For<ILoggedOnUser>();

			isolate.UseTestDouble<RemoveDayOffCommandHandler>().For<IHandleCommand<RemoveDayOffCommand>>();
		}

		[Test]
		public void ShouldDeleteDayOffOnGivenDay()
		{
			var date = new DateOnly(2018, 1, 11);
			var person = PersonFactory.CreatePersonWithId();
			PersonRepository.Has(person);

			var scenario = ScenarioFactory.CreateScenarioWithId("default", true);
			ScenarioRepository.Has(scenario);

			var dayOffTemplate = DayOffFactory.CreateDayOff(new Description("template")).WithId();
			DayOffTemplateRepository.Has(dayOffTemplate);
			var personAssignment = PersonAssignmentFactory.CreateAssignmentWithDayOff(person, scenario, date, dayOffTemplate).WithId();
			ScheduleStorage.Add(personAssignment);

			var trackId = Guid.NewGuid();
			var command = new RemoveDayOffCommand
			{
				Person = person,
				Date = date,
				TrackedCommandInfo = new TrackedCommandInfo
				{
					OperatedPersonId = Guid.NewGuid(),
					TrackId = trackId
				}
			};
			Target.Handle(command);

			personAssignment = PersonAssignmentRepository.LoadAll().Single(p => p.Person.Id == person.Id.Value);
			personAssignment.Date.Should().Be.EqualTo(date);
			personAssignment.DayOff().Should().Be.Null();
		}

		[Test]
		public void ShouldRaiseEvent()
		{
			var date = new DateOnly(2018, 1, 11);
			var person = PersonFactory.CreatePersonWithId();
			PersonRepository.Has(person);

			var scenario = ScenarioFactory.CreateScenarioWithId("default", true);
			ScenarioRepository.Has(scenario);

			var dayOffTemplate = DayOffFactory.CreateDayOff(new Description("template")).WithId();
			DayOffTemplateRepository.Has(dayOffTemplate);
			var personAssignment = PersonAssignmentFactory.CreateAssignmentWithDayOff(person, scenario, date, dayOffTemplate).WithId();
			ScheduleStorage.Add(personAssignment);

			var trackId = Guid.NewGuid();
			var command = new RemoveDayOffCommand
			{
				Person = person,
				Date = date,
				TrackedCommandInfo = new TrackedCommandInfo
				{
					OperatedPersonId = Guid.NewGuid(),
					TrackId = trackId
				}
			};
			Target.Handle(command);

			var theEvent = PersonAssignmentRepository.LoadAll().Single(p => p.Person.Id == person.Id.Value).PopAllEvents(null).Single(e => e is DayOffDeletedEvent) as DayOffDeletedEvent;
			theEvent.Date.Should().Be.EqualTo(date.Date);
			theEvent.PersonId.Should().Be.EqualTo(person.Id.Value);
			theEvent.CommandId.Should().Be.EqualTo(trackId);
			theEvent.ScenarioId.Should().Be.EqualTo(scenario.Id.Value);
		}

		[Test]
		public void ShouldUpdatePersonAccountWhenDeletingDayOffWithFullDayAbsenceWhenUsingDayTracker()
		{
			var date = new DateOnly(2018, 1, 11);
			var person = PersonFactory.CreatePersonWithPersonPeriod(date).WithId();
			LoggedOnUser.SetFakeLoggedOnUser(person);
			LoggedOnUser.SetDefaultTimeZone(TimeZoneInfo.Utc);
			PersonRepository.Has(person);

			var scenario = ScenarioFactory.CreateScenarioWithId("default", true);
			ScenarioRepository.Has(scenario);

			var existingAbsence = AbsenceFactory.CreateAbsence("gone").WithId();
			existingAbsence.Tracker = Tracker.CreateDayTracker();

			var fullDayAbsence = new AbsenceLayer(existingAbsence,
				new DateTimePeriod(new DateTime(2018, 1, 11, 0, 0, 0, DateTimeKind.Utc),
					new DateTime(2018, 1, 11, 23, 59, 0, DateTimeKind.Utc)));

			var fullDayPersonAbsence = new PersonAbsence(person, scenario, fullDayAbsence).WithId();
			var personAbsenceAccount = new PersonAbsenceAccount(person, existingAbsence).WithId();
			personAbsenceAccount.Add(new AccountDay(date) { Accrued = TimeSpan.FromDays(25) });

			AbsenceRepository.Add(existingAbsence);
			ScheduleStorage.Add(fullDayPersonAbsence);
			PersonAbsenceAccountRepository.Add(personAbsenceAccount);

			var dayOffTemplate = DayOffFactory.CreateDayOff(new Description("template")).WithId();
			DayOffTemplateRepository.Has(dayOffTemplate);
			var personAssignment = PersonAssignmentFactory.CreateAssignmentWithDayOff(person, scenario, date, dayOffTemplate).WithId();
			ScheduleStorage.Add(personAssignment);

			var trackId = Guid.NewGuid();
			var command = new RemoveDayOffCommand
			{
				Person = person,
				Date = date,
				TrackedCommandInfo = new TrackedCommandInfo
				{
					OperatedPersonId = Guid.NewGuid(),
					TrackId = trackId
				}
			};
			Target.Handle(command);

			var accounts = PersonAbsenceAccountRepository.Find(person);
			accounts.AllPersonAccounts().First().Remaining.Should().Be.EqualTo(TimeSpan.FromDays(24));
		}

		[Test]
		public void ShouldUpdatePersonAccountWhenDeletingDayOffWithFullDayAbsenceWhenUsingTimeTracker()
		{
			var date = new DateOnly(2018, 1, 11);
			var person = PersonFactory.CreatePersonWithPersonPeriod(date).WithId();
			LoggedOnUser.SetFakeLoggedOnUser(person);
			LoggedOnUser.SetDefaultTimeZone(TimeZoneInfo.Utc);
			PersonRepository.Has(person);

			var scenario = ScenarioFactory.CreateScenarioWithId("default", true);
			ScenarioRepository.Has(scenario);

			var existingAbsence = AbsenceFactory.CreateAbsence("gone").WithId();
			existingAbsence.InContractTime = true;
			existingAbsence.Tracker = Tracker.CreateTimeTracker();

			var fullDayAbsence = new AbsenceLayer(existingAbsence,
				new DateTimePeriod(new DateTime(2018, 1, 11, 0, 0, 0, DateTimeKind.Utc),
					new DateTime(2018, 1, 11, 23, 59, 0, DateTimeKind.Utc)));

			var fullDayPersonAbsence = new PersonAbsence(person, scenario, fullDayAbsence).WithId();
			var personAbsenceAccount = new PersonAbsenceAccount(person, existingAbsence).WithId();
			personAbsenceAccount.Add(new AccountTime(date) { Accrued = TimeSpan.FromHours(25) });

			AbsenceRepository.Add(existingAbsence);
			ScheduleStorage.Add(fullDayPersonAbsence);
			PersonAbsenceAccountRepository.Add(personAbsenceAccount);

			var dayOffTemplate = DayOffFactory.CreateDayOff(new Description("template")).WithId();
			DayOffTemplateRepository.Has(dayOffTemplate);
			var personAssignment = PersonAssignmentFactory.CreateAssignmentWithDayOff(person, scenario, date, dayOffTemplate).WithId();
			ScheduleStorage.Add(personAssignment);

			var trackId = Guid.NewGuid();
			var command = new RemoveDayOffCommand
			{
				Person = person,
				Date = date,
				TrackedCommandInfo = new TrackedCommandInfo
				{
					OperatedPersonId = Guid.NewGuid(),
					TrackId = trackId
				}
			};
			Target.Handle(command);

			personAssignment = PersonAssignmentRepository.LoadAll().Single(p => p.Person.Id == person.Id.Value);
			personAssignment.Date.Should().Be.EqualTo(date);
			personAssignment.DayOff().Should().Be.Null();

			var accounts = PersonAbsenceAccountRepository.Find(person);
			accounts.AllPersonAccounts().First().Remaining.Should().Be.EqualTo(TimeSpan.FromHours(17));
		}
	}
}
