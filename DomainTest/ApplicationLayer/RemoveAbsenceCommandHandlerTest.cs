using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Rhino.Mocks;
using SharpTestsEx;
using Teleopti.Ccc.Domain.AgentInfo.Requests;
using Teleopti.Ccc.Domain.ApplicationLayer.Commands;
using Teleopti.Ccc.Domain.ApplicationLayer.Events;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.PersonalAccount;
using Teleopti.Ccc.Domain.Scheduling.SaveSchedulePart;
using Teleopti.Ccc.Domain.Staffing;
using Teleopti.Ccc.Domain.UnitOfWork;
using Teleopti.Ccc.Infrastructure.Persisters.Schedules;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.TestCommon.FakeRepositories;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.ApplicationLayer
{
	[TestFixture]
	[TestWithStaticDependenciesAvoidUse]
	public class RemovePersonAbsenceCommandHandlerTest
	{
		private SaveSchedulePartService _saveSchedulePartService;
		private FakeScheduleStorage _scheduleStorage;
		private BusinessRulesForPersonalAccountUpdate _businessRulesForAccountUpdate;
		private PersonAbsenceRemover _personAbsenceRemover;
		private ICurrentScenario _scenario;
		private PersonAbsenceCreator _personAbsenceCreator;
		private ILoggedOnUser _loggedOnUser;
		private FakePersonAbsenceAccountRepository _personAbsenceAccountRepository;

		[SetUp]
		public void Setup()
		{
			_scenario = new FakeCurrentScenario();
			_personAbsenceAccountRepository = new FakePersonAbsenceAccountRepository();

			_businessRulesForAccountUpdate = new BusinessRulesForPersonalAccountUpdate(_personAbsenceAccountRepository,
				new SchedulingResultStateHolder());

			
			_scheduleStorage = new FakeScheduleStorage();
			var scheduleDifferenceSaver = new ScheduleDifferenceSaver(_scheduleStorage, new ThisUnitOfWork(new FakeUnitOfWork()));
			_saveSchedulePartService = new SaveSchedulePartService(scheduleDifferenceSaver, _personAbsenceAccountRepository,
				new DoNothingScheduleDayChangeCallBack(), new EmptyScheduleDayDifferenceSaver());

			_personAbsenceCreator = new PersonAbsenceCreator(_saveSchedulePartService, _businessRulesForAccountUpdate);

			var person = PersonFactory.CreatePersonWithApplicationRolesAndFunctions();
			_loggedOnUser = MockRepository.GenerateMock<ILoggedOnUser>();
			_loggedOnUser.Stub(x => x.CurrentUser()).Return(person);

			_personAbsenceRemover = new PersonAbsenceRemover(_businessRulesForAccountUpdate, _saveSchedulePartService, _personAbsenceCreator,
				_loggedOnUser,
				new CheckingPersonalAccountDaysProvider(_personAbsenceAccountRepository));
		}

		[Test]
		public void ShouldRemovePersonAbsenceFromScheduleDay()
		{
			var startDate = new DateTime(2015, 10, 1, 13, 0, 0, DateTimeKind.Utc);
			var endDate = new DateTime(2015, 10, 1, 17, 0, 0, DateTimeKind.Utc);
			var dateTimePeriod = new DateTimePeriod(startDate, endDate);

			var person = PersonFactory.CreatePersonWithId();
			var absenceLayer = new AbsenceLayer(new Absence(), dateTimePeriod);
			var personAbsence = new PersonAbsence(person, _scenario.Current(), absenceLayer).WithId();

			_scheduleStorage.Add(personAbsence);

			var target = new RemovePersonAbsenceCommandHandler(_personAbsenceRemover, _scheduleStorage, _scenario);

			var command = new RemovePersonAbsenceCommand
			{
				ScheduleDate = startDate,
				Person = person,
				PersonAbsence = personAbsence
			};

			target.Handle(command);

			Assert.That(_scheduleStorage.LoadAll().Any() == false);
		}


		[Test]
		public void ShouldReturnErrorMessages()
		{
			var startDate = new DateTime(2015, 10, 1, 13, 0, 0, DateTimeKind.Utc);
			var endDate = new DateTime(2015, 10, 1, 17, 0, 0, DateTimeKind.Utc);
			var dateTimePeriod = new DateTimePeriod(startDate, endDate);

			var person = PersonFactory.CreatePersonWithId();
			var absenceLayer = new AbsenceLayer(new Absence(), dateTimePeriod);
			var personAbsence = new PersonAbsence(person, _scenario.Current(), absenceLayer).WithId();

			_scheduleStorage.Add(personAbsence);

			var personAbsenceRemover = MockRepository.GenerateMock<IPersonAbsenceRemover>();
			var errorMessages = new List<string>
			{
				string.Format("Error message {0}", Guid.NewGuid()),
				string.Format("Error message {0}", Guid.NewGuid())
			};
			personAbsenceRemover.Stub(x => x.RemovePersonAbsence(new DateOnly(dateTimePeriod.StartDateTime), person, personAbsence, null))
				.IgnoreArguments().Return(errorMessages);

			var target = new RemovePersonAbsenceCommandHandler(personAbsenceRemover, _scheduleStorage, _scenario);

			var command = new RemovePersonAbsenceCommand
			{
				ScheduleDate = startDate,
				Person = person,
				PersonAbsence = personAbsence
			};

			target.Handle(command);

			var error = command.Errors;
			Assert.That(error != null);
			Assert.That(error.PersonId == person.Id.Value);
			Assert.That(error.PersonName == person.Name);
			Assert.That(error.ErrorMessages.Count() == errorMessages.Count);
			foreach (var message in errorMessages)
			{
				Assert.That(error.ErrorMessages.Contains(message));
			}
		}


		[Test]
		public void ShouldRaisePersonAbsenceRemovedEvent()
		{
			var startDate = new DateTime(2015, 10, 1, 13, 0, 0, DateTimeKind.Utc);
			var endDate = new DateTime(2015, 10, 1, 17, 0, 0, DateTimeKind.Utc);
			var dateTimePeriod = new DateTimePeriod(startDate, endDate);

			var person = PersonFactory.CreatePersonWithId();
			var absenceLayer = new AbsenceLayer(new Absence(), dateTimePeriod);
			var personAbsence = new PersonAbsence(person, _scenario.Current(), absenceLayer).WithId();

			_scheduleStorage.Add(personAbsence);

			var target = new RemovePersonAbsenceCommandHandler(_personAbsenceRemover, _scheduleStorage, _scenario);

			var operatedPersonId = Guid.NewGuid();
			var trackId = Guid.NewGuid();
			var command = new RemovePersonAbsenceCommand
			{
				Person = person,
				PersonAbsence = personAbsence,
				ScheduleDate = startDate,
				TrackedCommandInfo = new TrackedCommandInfo
				{
					OperatedPersonId = operatedPersonId,
					TrackId = trackId
				}
			};

			target.Handle(command);
			var @event = personAbsence.PopAllEvents().Single() as PersonAbsenceRemovedEvent;
			@event.PersonId.Should().Be(personAbsence.Person.Id.Value);
			@event.ScenarioId.Should().Be(personAbsence.Scenario.Id.Value);
			@event.StartDateTime.Should().Be(personAbsence.Layer.Period.StartDateTime);
			@event.EndDateTime.Should().Be(personAbsence.Layer.Period.EndDateTime);
			@event.InitiatorId.Should().Be(operatedPersonId);
			@event.CommandId.Should().Be(trackId);
			@event.LogOnBusinessUnitId.Should().Be(_scenario.Current().BusinessUnit.Id.GetValueOrDefault());
		}

		[Test]
		public void ShouldUpdateAllPersonalAccountsWhenAbsenceIsRemoved()
		{
			var dateTimePeriod = new DateTimePeriod(2016, 08, 17, 00, 2016, 08, 18, 23);

			var person = PersonFactory.CreatePersonWithId();
			var absenceLayer = new AbsenceLayer(new Absence(), dateTimePeriod);
			var personAbsence = new PersonAbsence(person, _scenario.Current(), absenceLayer).WithId();

			createShiftsForPeriod(person, dateTimePeriod);
			_scheduleStorage.Add(personAbsence);

			var accountDay1 = createAccountDay(new DateOnly(2015, 12, 1), TimeSpan.FromDays(0), TimeSpan.FromDays(5),
				TimeSpan.FromDays(1));
			var accountDay2 = createAccountDay(new DateOnly(2016, 08, 18), TimeSpan.FromDays(0), TimeSpan.FromDays(3),
				TimeSpan.FromDays(1));
			var account = PersonAbsenceAccountFactory.CreatePersonAbsenceAccount(person, absenceLayer.Payload, accountDay1,
				accountDay2);

			setAbsenceRemoverForCheckingAccount(person, account);

			var target = new RemovePersonAbsenceCommandHandler(_personAbsenceRemover, _scheduleStorage, _scenario);

			var command = new RemovePersonAbsenceCommand
			{
				ScheduleDate = dateTimePeriod.StartDateTime,
				Person = person,
				PersonAbsence = personAbsence
			};

			target.Handle(command);

			Assert.AreEqual(5, accountDay1.Remaining.TotalDays);
			Assert.AreEqual(3, accountDay2.Remaining.TotalDays);
		}

		private void setAbsenceRemoverForCheckingAccount(IPerson person, IPersonAbsenceAccount account)
		{
			_personAbsenceAccountRepository.Add(account);
			var schedulingResultStateHolder = new SchedulingResultStateHolder
			{
				AllPersonAccounts = new Dictionary<IPerson, IPersonAccountCollection>
				{
					{person, new PersonAccountCollection(person) {account}}
				}
			};
			_businessRulesForAccountUpdate = new BusinessRulesForPersonalAccountUpdate(_personAbsenceAccountRepository,
				schedulingResultStateHolder);
			_personAbsenceRemover = new PersonAbsenceRemover(_businessRulesForAccountUpdate, _saveSchedulePartService,
				_personAbsenceCreator,
				_loggedOnUser,
				new CheckingPersonalAccountDaysProvider(_personAbsenceAccountRepository));
		}

		private AccountDay createAccountDay(DateOnly startDate, TimeSpan balanceIn, TimeSpan accrued, TimeSpan balance)
		{
			return new AccountDay(startDate)
			{
				BalanceIn = balanceIn,
				Accrued = accrued,
				Extra = TimeSpan.FromDays(0),
				LatestCalculatedBalance = balance
			};
		}

		private void createShiftsForPeriod(IPerson person, DateTimePeriod period)
		{
			foreach (var day in period.WholeDayCollection(TimeZoneInfo.Utc))
			{
				_scheduleStorage.Add(createAssignment(person, day.StartDateTime, day.EndDateTime, _scenario.Current()));
			}
		}

		private static IPersonAssignment createAssignment(IPerson person, DateTime startDate, DateTime endDate, IScenario scenario)
		{
			return PersonAssignmentFactory.CreateAssignmentWithMainShiftAndPersonalShift(person,
				scenario, new DateTimePeriod(startDate, endDate));
		}
	}
}