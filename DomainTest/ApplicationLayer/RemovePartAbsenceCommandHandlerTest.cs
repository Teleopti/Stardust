using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.ApplicationLayer.Commands;
using Teleopti.Ccc.Domain.ApplicationLayer.Events;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.SaveSchedulePart;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Scheduling.Assignment;
using Teleopti.Ccc.Domain.Scheduling.Rules;
using Teleopti.Ccc.Domain.Security.AuthorizationData;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.TestCommon.FakeRepositories;
using Teleopti.Ccc.TestCommon.IoC;


namespace Teleopti.Ccc.DomainTest.ApplicationLayer
{
	[DomainTest]
	public class RemovePartPersonAbsenceCommandHandlerTest : IIsolateSystem
	{
		public FakePersonAbsenceRepository PersonAbsenceRepository;
		public FakeScenarioRepository ScenarioRepository;
		public FullPermission Permission;
		public RemovePartPersonAbsenceCommandHandler Target;

		public void Isolate(IIsolate isolate)
		{
			isolate.UseTestDouble<RemovePartPersonAbsenceCommandHandler>().For<RemovePartPersonAbsenceCommandHandler>();
			isolate.UseTestDouble<businessRulesForPersonalAccountUpdateWithNewPersonAccountRuleHaltModify>()
				.For<BusinessRulesForPersonalAccountUpdate>();
		}

		[Test]
		public void NothingShouldBeDoneIfPeriodToRemoveIsTotallyEarlierThanAbsencePeriod()
		{
			var scenario = ScenarioRepository.Has("Default");
			var periodForAbsence = new DateTimePeriod(
				new DateTime(2016, 03, 2, 13, 0, 0, DateTimeKind.Utc),
				new DateTime(2016, 03, 3, 17, 0, 0, DateTimeKind.Utc));

			var periodToRemove = new DateTimePeriod(
				new DateTime(2016, 03, 1, 0, 0, 0, DateTimeKind.Utc),
				new DateTime(2016, 03, 1, 22, 0, 0, DateTimeKind.Utc));

			var allPersonAbsences = removePartPersonAbsence(scenario, periodForAbsence, periodToRemove).ToList();
			Assert.That(allPersonAbsences.Count == 1);
			Assert.That(
				allPersonAbsences.Any(
					pa => pa.Period.StartDateTime == periodForAbsence.StartDateTime
						  && pa.Period.EndDateTime == periodForAbsence.EndDateTime));
		}

		[Test]
		public void NothingShouldBeDoneIfPeriodToRemoveIsTotallyLaterThanAbsencePeriod()
		{
			var scenario = ScenarioRepository.Has("Default");
			var periodForAbsence = new DateTimePeriod(
				new DateTime(2016, 03, 2, 13, 0, 0, DateTimeKind.Utc),
				new DateTime(2016, 03, 3, 17, 0, 0, DateTimeKind.Utc));

			var periodToRemove = new DateTimePeriod(
				new DateTime(2016, 03, 4, 0, 0, 0, DateTimeKind.Utc),
				new DateTime(2016, 03, 4, 22, 0, 0, DateTimeKind.Utc));

			var allPersonAbsences = removePartPersonAbsence(scenario, periodForAbsence, periodToRemove).ToList();
			Assert.That(allPersonAbsences.Count == 1);
			Assert.That(
				allPersonAbsences.Any(
					pa => pa.Period.StartDateTime == periodForAbsence.StartDateTime
						  && pa.Period.EndDateTime == periodForAbsence.EndDateTime));
		}

		[Test]
		public void ShouldRemoveEntirePersonAbsenceWhenPeriodToRemoveCoverdAbsencePeriod()
		{
			var scenario = ScenarioRepository.Has("Default");
			var periodForAbsence = new DateTimePeriod(
				new DateTime(2016, 03, 2, 13, 0, 0, DateTimeKind.Utc),
				new DateTime(2016, 03, 3, 17, 0, 0, DateTimeKind.Utc));

			var periodToRemove = new DateTimePeriod(
				new DateTime(2016, 03, 1, 0, 0, 0, DateTimeKind.Utc),
				new DateTime(2016, 03, 4, 8, 0, 0, DateTimeKind.Utc));

			var allPersonAbsences = removePartPersonAbsence(scenario, periodForAbsence, periodToRemove).ToList();
			Assert.That(!allPersonAbsences.Any());
		}

		[Test]
		public void ShouldRemovePartPersonAbsenceWhenAbsencePeriodCoveredPeriodToRemove()
		{
			var scenario = ScenarioRepository.Has("Default");
			var periodForAbsence = new DateTimePeriod(
				new DateTime(2016, 03, 1, 13, 0, 0, DateTimeKind.Utc),
				new DateTime(2016, 03, 4, 17, 0, 0, DateTimeKind.Utc));

			var periodToRemove = new DateTimePeriod(
				new DateTime(2016, 03, 2, 0, 0, 0, DateTimeKind.Utc),
				new DateTime(2016, 03, 3, 8, 0, 0, DateTimeKind.Utc));

			var allPersonAbsences = removePartPersonAbsence(scenario, periodForAbsence, periodToRemove).ToList();
			Assert.That(allPersonAbsences.Count == 2);
			Assert.That(
				allPersonAbsences.Any(
					pa => pa.Period.StartDateTime == periodForAbsence.StartDateTime
						  && pa.Period.EndDateTime == periodToRemove.StartDateTime));
			Assert.That(
				allPersonAbsences.Any(
					pa => pa.Period.StartDateTime == periodToRemove.EndDateTime
						  && pa.Period.EndDateTime == periodForAbsence.EndDateTime));
		}

		[Test]
		public void ShouldCreateCorrectNewPersonAbsenceWhenPeriodToRemoveIncludeAbsencePeriodEnd()
		{
			var scenario = ScenarioRepository.Has("Default");
			var periodForAbsence = new DateTimePeriod(
				new DateTime(2016, 03, 1, 13, 0, 0, DateTimeKind.Utc),
				new DateTime(2016, 03, 3, 17, 0, 0, DateTimeKind.Utc));

			var periodToRemove = new DateTimePeriod(
				new DateTime(2016, 03, 3, 0, 0, 0, DateTimeKind.Utc),
				new DateTime(2016, 03, 3, 22, 0, 0, DateTimeKind.Utc));

			var allPersonAbsences = removePartPersonAbsence(scenario, periodForAbsence, periodToRemove).ToList();
			Assert.That(allPersonAbsences.Count == 1);
			Assert.That(
				allPersonAbsences.Any(
					pa => pa.Period.StartDateTime == periodForAbsence.StartDateTime
						  && pa.Period.EndDateTime == periodToRemove.StartDateTime));
		}

		[Test]
		public void ShouldCreateCorrectNewPersonAbsenceWhenPeriodToRemoveIncludeAbsencePeriodStart()
		{
			var scenario = ScenarioRepository.Has("Default");
			var periodForAbsence = new DateTimePeriod(
				new DateTime(2016, 03, 2, 13, 0, 0, DateTimeKind.Utc),
				new DateTime(2016, 03, 3, 17, 0, 0, DateTimeKind.Utc));

			var periodToRemove = new DateTimePeriod(
				new DateTime(2016, 03, 1, 0, 0, 0, DateTimeKind.Utc),
				new DateTime(2016, 03, 3, 0, 0, 0, DateTimeKind.Utc));

			var allPersonAbsences = removePartPersonAbsence(scenario, periodForAbsence, periodToRemove).ToList();
			Assert.That(allPersonAbsences.Count == 1);
			Assert.That(
				allPersonAbsences.Any(
					pa => pa.Period.StartDateTime == periodToRemove.EndDateTime
						  && pa.Period.EndDateTime == periodForAbsence.EndDateTime));
		}

		[Test]
		public void ShouldRaisePersonAbsenceRemovedEvent()
		{
			var scenario = ScenarioRepository.Has("Default");
			var person = PersonFactory.CreatePersonWithApplicationRolesAndFunctions();
			person.SetId(Guid.NewGuid());
			
			var startDate = new DateTime(2016, 01, 01, 00, 00, 00, DateTimeKind.Utc);
			var endDate = new DateTime(2016, 01, 01, 01, 00, 00, DateTimeKind.Utc);
			var period = new DateTimePeriod(startDate, endDate);
			var layer = new AbsenceLayer(new Absence(), period);
			var personAbsence = new PersonAbsence(person, scenario, layer);
			personAbsence.SetId(Guid.NewGuid());
			
			var operatedPersonId = Guid.NewGuid();
			var trackId = Guid.NewGuid();
			var command = new RemovePartPersonAbsenceCommand
			{
				ScheduleDate = startDate,
				Person = person,
				PersonAbsence = personAbsence,
				PeriodToRemove = period.ChangeEndTime(new TimeSpan(0, 0, 1)),
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
			@event.LogOnBusinessUnitId.Should().Be(scenario.BusinessUnit.Id.GetValueOrDefault());
		}

		[Test]
		public void ShouldReturnErrorMessages()
		{
			Permission.AddToBlackList(DefinedRaptorApplicationFunctionPaths.ModifyWriteProtectedSchedule);

			var scenario = ScenarioRepository.Has("Default");
			var startDate = new DateTime(2016, 01, 01, 00, 00, 00, DateTimeKind.Utc);
			var endDate = new DateTime(2016, 01, 01, 01, 00, 00, DateTimeKind.Utc);
			var period = new DateTimePeriod(startDate, endDate);
			var layer = new AbsenceLayer(new Absence(), period);
			var person = PersonFactory.CreatePersonWithId();
			var personAbsence = new PersonAbsence(person, scenario, layer);
			personAbsence.SetId(Guid.NewGuid());

			person.PersonWriteProtection.PersonWriteProtectedDate = new DateOnly(2016,2,1);
			var periodToRemove = period.ChangeEndTime(new TimeSpan(0, 0, 1));
			
			var command = new RemovePartPersonAbsenceCommand
			{
				ScheduleDate = startDate,
				Person = person,
				PersonAbsence = personAbsence,
				PeriodToRemove = periodToRemove
			};

			Target.Handle(command);

			var error = command.Errors;
			Assert.That(error != null);
			Assert.That(error.PersonId == person.Id.Value);
			Assert.That(error.PersonName == person.Name);
			Assert.That(error.ErrorMessages.Count() == 1);
		}
		
		private IEnumerable<IPersistableScheduleData> removePartPersonAbsence(IScenario scenario, DateTimePeriod periodForAbsence,
			DateTimePeriod periodToRemove)
		{
			var person = PersonFactory.CreatePersonWithApplicationRolesAndFunctions();

			var absenceLayer = new AbsenceLayer(new Absence(), periodForAbsence);
			var personAbsence = new PersonAbsence(person, scenario, absenceLayer);
			personAbsence.SetId(Guid.NewGuid());
			PersonAbsenceRepository.Add(personAbsence);
			
			var command = new RemovePartPersonAbsenceCommand
			{
				ScheduleDate = periodToRemove.StartDateTime.Date,
				Person = person,
				PersonAbsence = personAbsence,
				PeriodToRemove = periodToRemove
			};

			Target.Handle(command);

			var allPersonAbsences = PersonAbsenceRepository.LoadAll();
			return allPersonAbsences;
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