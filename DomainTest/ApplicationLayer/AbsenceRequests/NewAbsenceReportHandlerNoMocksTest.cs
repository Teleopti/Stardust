using System;
using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.ApplicationLayer.AbsenceRequests;
using Teleopti.Ccc.Domain.ApplicationLayer.Events;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Scheduling.PersonalAccount;
using Teleopti.Ccc.Domain.Tracking;
using Teleopti.Ccc.Domain.WorkflowControl;
using Teleopti.Ccc.IocCommon;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.TestCommon.FakeRepositories;
using Teleopti.Ccc.TestCommon.IoC;


namespace Teleopti.Ccc.DomainTest.ApplicationLayer.AbsenceRequests
{
	[TestFixture]
	[DomainTest]
	public class NewAbsenceReportHandlerNoMocksTest : IIsolateSystem
	{
		public FakeScenarioRepository ScenarioRepository;
		public IScheduleStorage ScheduleRepository;
		public FakePersonAbsenceAccountRepository PersonAbsenceAccountRepository;
		public FakePersonRepository PersonRepository;
		public FakePersonAssignmentRepository PersonAssignmentRepository;
		public NewAbsenceReport Target;
		
		[Test]
		public void VerifyAbsenceIsCreatedForAbsenceReport()
		{
			var scenario = ScenarioRepository.Has("Default");
			var absence = AbsenceFactory.CreateAbsenceWithId();

			var startDate = new DateTime(2016, 02, 17, 0, 0, 0, DateTimeKind.Utc);
			var endDate = new DateTime(2016, 02, 19, 23, 59, 0, DateTimeKind.Utc);

			var person = setupPerson(startDate, endDate, absence);
			
			var absenceReport = new NewAbsenceReportCreatedEvent
			{
				RequestedDate = startDate,
				PersonId = person.Id.GetValueOrDefault(),
				AbsenceId = absence.Id.GetValueOrDefault()
			};

			Target.Handle(absenceReport);

			var scheduleLoadOptions = new ScheduleDictionaryLoadOptions(false, false);
			var schedules = ScheduleRepository.FindSchedulesForPersonOnlyInGivenPeriod(person, scheduleLoadOptions, new DateTimePeriod(startDate, startDate.AddDays(1)), scenario);
			var scheduleDay = schedules.SchedulesForDay(new DateOnly(startDate)).FirstOrDefault();
			var personAbsence = scheduleDay.PersonAbsenceCollection().SingleOrDefault(abs => abs.Layer.Payload == absence && abs.Person == person);

			Assert.IsNotNull(personAbsence);
		}

		[Test]
		public void VerifyFullDayAbsenceAddedEventIsPopulatedWhenAbsenceIsCreatedForAbsenceReport()
		{
			var scenario = ScenarioRepository.Has("Default");
			var absence = AbsenceFactory.CreateAbsenceWithId();

			var startDate = new DateTime(2016,02,17,0,0,0,DateTimeKind.Utc);
			var endDate = new DateTime(2016,02,17,23,59,0,DateTimeKind.Utc);

			var person = setupPerson(startDate,endDate,absence);
			
			var absenceReport = new NewAbsenceReportCreatedEvent
			{
				RequestedDate = startDate,
				PersonId = person.Id.GetValueOrDefault(),
				AbsenceId = absence.Id.GetValueOrDefault()
			};

			Target.Handle(absenceReport);

			var scheduleLoadOptions = new ScheduleDictionaryLoadOptions(false,false);
			var schedules = ScheduleRepository.FindSchedulesForPersonOnlyInGivenPeriod(person,scheduleLoadOptions,new DateTimePeriod(startDate,startDate.AddDays(1)),scenario);
			var scheduleDay = schedules.SchedulesForDay(new DateOnly(startDate)).FirstOrDefault();
			var personAbsence = scheduleDay.PersonAbsenceCollection().SingleOrDefault(abs => abs.Layer.Payload == absence && abs.Person == person);


			var @event = personAbsence.PopAllEvents(null).Single() as FullDayAbsenceAddedEvent;

			@event.Should().Not.Be.Null();
			@event.StartDateTime.Should().Be.EqualTo(personAbsence.Layer.Period.StartDateTime);
			@event.EndDateTime.Should().Be.EqualTo(personAbsence.Layer.Period.EndDateTime);
			@event.AbsenceId.Should().Be.EqualTo(absenceReport.AbsenceId);
			@event.PersonId.Should().Be.EqualTo(absenceReport.PersonId);
		}


		[Test]
		public void VerifyPersonAccountIsUpdated()
		{
			ScenarioRepository.Has("Default");
			var absence = AbsenceFactory.CreateAbsenceWithId();

			var startDate = new DateTime(2016, 02, 17, 0, 0, 0, DateTimeKind.Utc);
			var endDate = new DateTime(2016, 02, 19, 23, 59, 0, DateTimeKind.Utc);

			var person = setupPerson(startDate, endDate, absence);
			
			var absenceReport = new NewAbsenceReportCreatedEvent
			{
				RequestedDate = startDate,
				PersonId = person.Id.GetValueOrDefault(),
				AbsenceId = absence.Id.GetValueOrDefault()
			};

			var accountDay = new AccountDay(new DateOnly(2015, 12, 1))
			{
				BalanceIn = TimeSpan.FromDays(5),
				Accrued = TimeSpan.FromDays(20),
				Extra = TimeSpan.FromDays(0)
			};

			createPersonAbsenceAccount(person, absence, accountDay);

			Target.Handle(absenceReport);

			Assert.AreEqual(24, accountDay.Remaining.TotalDays);
		}
		
		private IPerson setupPerson(DateTime startDate, DateTime endDate, IAbsence absence)
		{
			var person = PersonFactory.CreatePersonWithId();
			PersonRepository.Add(person);
			
			person.WorkflowControlSet = createWorkFlowControlSet(startDate, endDate, absence);

			var assignmentOne = createAssignment(person, startDate, startDate.AddHours(8), ScenarioRepository);
			PersonAssignmentRepository.Has(assignmentOne);
			return person;
		}

		private static WorkflowControlSet createWorkFlowControlSet(DateTime startDate, DateTime endDate, IAbsence absence)
		{
			var workflowControlSet = new WorkflowControlSet();
			var dateOnlyPeriod = new DateOnlyPeriod(new DateOnly(startDate), new DateOnly(endDate));

			var absenceRequestOpenPeriod = new AbsenceRequestOpenDatePeriod
			{
				Absence = absence,
				Period = dateOnlyPeriod,
				OpenForRequestsPeriod = dateOnlyPeriod
			};

			workflowControlSet.InsertPeriod(absenceRequestOpenPeriod, 0);
			workflowControlSet.AllowedAbsencesForReport = new[] { absence };

			return workflowControlSet;
		}

		private static IPersonAssignment createAssignment(IPerson person, DateTime startDate, DateTime endDate, IScenarioRepository currentScenario)
		{
			return PersonAssignmentFactory.CreateAssignmentWithMainShiftAndPersonalShift(person,
				currentScenario.LoadDefaultScenario(), new DateTimePeriod(startDate, endDate));
		}

		private void createPersonAbsenceAccount(IPerson person, IAbsence absence, IAccount accountDay)
		{
			var personAbsenceAccount = new PersonAbsenceAccount(person, absence);
			personAbsenceAccount.Absence.Tracker = Tracker.CreateDayTracker();
			personAbsenceAccount.Add(accountDay);

			PersonAbsenceAccountRepository.Add(personAbsenceAccount);
		}

		public void Isolate(IIsolate isolate)
		{
			isolate.UseTestDouble<NewAbsenceReport>().For<NewAbsenceReport>();
		}
	}
}
