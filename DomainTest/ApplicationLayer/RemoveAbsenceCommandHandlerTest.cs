using System;
using System.Linq;
using NUnit.Framework;
using Rhino.Mocks;
using SharpTestsEx;
using Teleopti.Ccc.Domain.ApplicationLayer.Commands;
using Teleopti.Ccc.Domain.ApplicationLayer.Events;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.SaveSchedulePart;
using Teleopti.Ccc.Infrastructure.Persisters.Schedules;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.TestCommon.FakeRepositories;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.ApplicationLayer
{
	[TestFixture]
	public class RemovePersonAbsenceCommandHandlerTest
	{

		private SaveSchedulePartService _saveSchedulePartService;
		private FakeScheduleRepository _scheduleRepository;
		private BusinessRulesForPersonalAccountUpdate _businessRulesForAccountUpdate;


		[SetUp]
		public void Setup()
		{
			var personAbsenceAccountRepository = new FakePersonAbsenceAccountRepository();
			_businessRulesForAccountUpdate = new BusinessRulesForPersonalAccountUpdate(personAbsenceAccountRepository, new SchedulingResultStateHolder());
			_scheduleRepository = new FakeScheduleRepository();
			var scheduleDifferenceSaver = new ScheduleDifferenceSaver(_scheduleRepository);
			_saveSchedulePartService = new SaveSchedulePartService(scheduleDifferenceSaver, personAbsenceAccountRepository);
		}

		[Test]
		public void ShouldRemovePersonAbsenceFromScheduleDay()
		{
			var dateTimePeriod = new DateTimePeriod (
				new DateTime(2015,10,1,13,0,0, DateTimeKind.Utc), 
				new DateTime(2015,10,1,17,0,0, DateTimeKind.Utc));

			var absenceLayer = new AbsenceLayer (new Absence(), dateTimePeriod );
			var personAbsence = new PersonAbsence(PersonFactory.CreatePersonWithId(), new FakeCurrentScenario().Current(), absenceLayer);
			personAbsence.SetId(Guid.Empty);
			_scheduleRepository.Add(personAbsence);
			
			var personAbsenceRepository = new FakeWriteSideRepository<IPersonAbsence>() { personAbsence };

			var target = new RemovePersonAbsenceCommandHandler(personAbsenceRepository, _scheduleRepository, _businessRulesForAccountUpdate, _saveSchedulePartService);

			var command = new RemovePersonAbsenceCommand
			{
				PersonAbsenceIds = new[] {personAbsence.Id.Value}
			};

			target.Handle(command);

			Assert.That(_scheduleRepository.LoadAll().Any() == false);

		}

		[Test]
		public void ShouldRaisePersonAbsenceRemovedEvent()
		{
			var scenario = new FakeCurrentScenario().Current();
			var personAbsence = new PersonAbsence(PersonFactory.CreatePersonWithId(), scenario, MockRepository.GenerateMock<IAbsenceLayer>());
			personAbsence.SetId(Guid.Empty);

			var personAbsenceRepository = new FakeWriteSideRepository<IPersonAbsence>() { personAbsence };
			
			var target = new RemovePersonAbsenceCommandHandler(personAbsenceRepository, _scheduleRepository, _businessRulesForAccountUpdate, _saveSchedulePartService);

			var operatedPersonId = Guid.NewGuid();
			var trackId = Guid.NewGuid();
			var command = new RemovePersonAbsenceCommand
			{
				PersonAbsenceIds = new[] {personAbsence.Id.Value},
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
			@event.TrackId.Should().Be(trackId);
			@event.LogOnBusinessUnitId.Should().Be(scenario.BusinessUnit.Id.GetValueOrDefault());
		}
	}
}