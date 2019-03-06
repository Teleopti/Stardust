using System;
using System.Collections.ObjectModel;
using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.ApplicationLayer.Commands;
using Teleopti.Ccc.Domain.ApplicationLayer.Events;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.Assignment;
using Teleopti.Ccc.Domain.Scheduling.PersonalAccount;
using Teleopti.Ccc.Domain.Scheduling.Rules;
using Teleopti.Ccc.Domain.Scheduling.SaveSchedulePart;
using Teleopti.Ccc.Domain.Security.AuthorizationData;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.TestCommon.FakeRepositories;
using Teleopti.Ccc.TestCommon.IoC;


namespace Teleopti.Ccc.DomainTest.ApplicationLayer
{
	[DomainTest]
	[NoDefaultData]
	public class RemovePersonAbsenceCommandHandlerTest : IIsolateSystem
	{
		public FakePersonAssignmentRepository PersonAssignmentRepository;
		public FakePersonAbsenceRepository PersonAbsenceRepository;
		public FakePersonAbsenceAccountRepository PersonAbsenceAccountRepository;
		public FakeScenarioRepository ScenarioRepository;
		public FullPermission Permission;
		public RemovePersonAbsenceCommandHandler Target;

		public void Isolate(IIsolate isolate)
		{
			isolate.UseTestDouble<RemovePersonAbsenceCommandHandler>().For<RemovePersonAbsenceCommandHandler>();
			isolate.UseTestDouble<businessRulesForPersonalAccountUpdateWithNewPersonAccountRuleHaltModify>()
				.For<BusinessRulesForPersonalAccountUpdate>();
		}

		[Test]
		public void ShouldRemovePersonAbsenceFromScheduleDay()
		{
			var scenario = ScenarioRepository.Has("Default");
			var startDate = new DateTime(2015, 10, 1, 13, 0, 0, DateTimeKind.Utc);
			var endDate = new DateTime(2015, 10, 1, 17, 0, 0, DateTimeKind.Utc);
			var dateTimePeriod = new DateTimePeriod(startDate, endDate);

			var person = PersonFactory.CreatePersonWithId();
			var absenceLayer = new AbsenceLayer(new Absence(), dateTimePeriod);
			var personAbsence = new PersonAbsence(person, scenario, absenceLayer).WithId();

			PersonAbsenceRepository.Add(personAbsence);
			
			var command = new RemovePersonAbsenceCommand
			{
				ScheduleDate = startDate,
				Person = person,
				PersonAbsence = personAbsence
			};

			Target.Handle(command);

			Assert.That(PersonAssignmentRepository.LoadAll().Any() == false);
		}


		[Test]
		public void ShouldReturnErrorMessages()
		{
			Permission.AddToBlackList(DefinedRaptorApplicationFunctionPaths.ModifyWriteProtectedSchedule);
			var scenario = ScenarioRepository.Has("Default");
			var startDate = new DateTime(2015, 10, 1, 13, 0, 0, DateTimeKind.Utc);
			var endDate = new DateTime(2015, 10, 1, 17, 0, 0, DateTimeKind.Utc);
			var dateTimePeriod = new DateTimePeriod(startDate, endDate);

			var person = PersonFactory.CreatePersonWithId();
			var absenceLayer = new AbsenceLayer(new Absence(), dateTimePeriod);
			var personAbsence = new PersonAbsence(person, scenario, absenceLayer).WithId();

			PersonAbsenceRepository.Add(personAbsence);
			person.PersonWriteProtection.PersonWriteProtectedDate = new DateOnly(2015,11,1);
			
			var command = new RemovePersonAbsenceCommand
			{
				ScheduleDate = startDate,
				Person = person,
				PersonAbsence = personAbsence
			};

			Target.Handle(command);

			var error = command.Errors;
			Assert.That(error != null);
			Assert.That(error.PersonId == person.Id.Value);
			Assert.That(error.PersonName == person.Name);
			Assert.That(error.ErrorMessages.Count() == 1);
		}


		[Test]
		public void ShouldRaisePersonAbsenceRemovedEvent()
		{
			var scenario = ScenarioRepository.Has("Default");
			var startDate = new DateTime(2015, 10, 1, 13, 0, 0, DateTimeKind.Utc);
			var endDate = new DateTime(2015, 10, 1, 17, 0, 0, DateTimeKind.Utc);
			var dateTimePeriod = new DateTimePeriod(startDate, endDate);

			var person = PersonFactory.CreatePersonWithId();
			var absenceLayer = new AbsenceLayer(new Absence(), dateTimePeriod);
			var personAbsence = new PersonAbsence(person, scenario, absenceLayer).WithId();

			PersonAbsenceRepository.Add(personAbsence);
			
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

			Target.Handle(command);
			var @event = personAbsence.PopAllEvents(null).Single() as PersonAbsenceRemovedEvent;
			@event.PersonId.Should().Be(personAbsence.Person.Id.Value);
			@event.ScenarioId.Should().Be(personAbsence.Scenario.Id.Value);
			@event.StartDateTime.Should().Be(personAbsence.Layer.Period.StartDateTime);
			@event.EndDateTime.Should().Be(personAbsence.Layer.Period.EndDateTime);
			@event.InitiatorId.Should().Be(operatedPersonId);
			@event.CommandId.Should().Be(trackId);
			@event.LogOnBusinessUnitId.Should().Be(scenario.GetOrFillWithBusinessUnit_DONTUSE().Id.GetValueOrDefault());
		}

		[Test]
		public void ShouldUpdateAllPersonalAccountsWhenAbsenceIsRemoved()
		{
			var scenario = ScenarioRepository.Has("Default");
			var dateTimePeriod = new DateTimePeriod(2016, 08, 17, 00, 2016, 08, 18, 23);

			var person = PersonFactory.CreatePersonWithId();
			var absenceLayer = new AbsenceLayer(new Absence(), dateTimePeriod);
			var personAbsence = new PersonAbsence(person, scenario, absenceLayer).WithId();

			createShiftsForPeriod(scenario, person, dateTimePeriod);
			PersonAbsenceRepository.Add(personAbsence);

			var accountDay1 = createAccountDay(new DateOnly(2015, 12, 1), TimeSpan.FromDays(0), TimeSpan.FromDays(5),
				TimeSpan.FromDays(1));
			var accountDay2 = createAccountDay(new DateOnly(2016, 08, 18), TimeSpan.FromDays(0), TimeSpan.FromDays(3),
				TimeSpan.FromDays(1));
			var account = PersonAbsenceAccountFactory.CreatePersonAbsenceAccount(person, absenceLayer.Payload, accountDay1,
				accountDay2);

			setAbsenceRemoverForCheckingAccount(account);
			
			var command = new RemovePersonAbsenceCommand
			{
				ScheduleDate = dateTimePeriod.StartDateTime,
				Person = person,
				PersonAbsence = personAbsence
			};

			Target.Handle(command);

			Assert.AreEqual(5, accountDay1.Remaining.TotalDays);
			Assert.AreEqual(3, accountDay2.Remaining.TotalDays);
		}

		private void setAbsenceRemoverForCheckingAccount(IPersonAbsenceAccount account)
		{
			PersonAbsenceAccountRepository.Add(account);
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

		private void createShiftsForPeriod(IScenario scenario, IPerson person, DateTimePeriod period)
		{
			foreach (var day in period.WholeDayCollection(TimeZoneInfo.Utc))
			{
				PersonAssignmentRepository.Add(createAssignment(person, day.StartDateTime, day.EndDateTime, scenario));
			}
		}

		private static IPersonAssignment createAssignment(IPerson person, DateTime startDate, DateTime endDate, IScenario scenario)
		{
			return PersonAssignmentFactory.CreateAssignmentWithMainShiftAndPersonalShift(person,
				scenario, new DateTimePeriod(startDate, endDate));
		}

		private class
			businessRulesForPersonalAccountUpdateWithNewPersonAccountRuleHaltModify : BusinessRulesForPersonalAccountUpdate,
				IBusinessRulesForPersonalAccountUpdate
		{
			private readonly IPersonAbsenceAccountRepository _personAbsenceAccountRepository;
			private readonly ISchedulingResultStateHolder _schedulingResultStateHolder;

			public businessRulesForPersonalAccountUpdateWithNewPersonAccountRuleHaltModify(
				IPersonAbsenceAccountRepository personAbsenceAccountRepository,
				ISchedulingResultStateHolder schedulingResultStateHolder) : base(personAbsenceAccountRepository,
				schedulingResultStateHolder)
			{
				_personAbsenceAccountRepository = personAbsenceAccountRepository;
				_schedulingResultStateHolder = schedulingResultStateHolder;
			}

			public new INewBusinessRuleCollection FromScheduleRange(IScheduleRange scheduleRange)
			{
				var personAccounts = _personAbsenceAccountRepository.FindByUsers(new Collection<IPerson> { scheduleRange.Person });
				var rules = NewBusinessRuleCollection.MinimumAndPersonAccount(_schedulingResultStateHolder, personAccounts);
				((IValidateScheduleRange)scheduleRange).ValidateBusinessRules(rules);
				return rules;
			}
		}
	}
}