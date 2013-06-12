using System;
using System.Linq;
using NUnit.Framework;
using Rhino.Mocks;
using SharpTestsEx;
using Teleopti.Ccc.Domain.ApplicationLayer;
using Teleopti.Ccc.Domain.ApplicationLayer.Commands;
using Teleopti.Ccc.Domain.ApplicationLayer.Events;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.ApplicationLayer
{
	[TestFixture]
	public class RemovePersonAbsenceCommandHandlerTest
	{
		[Test]
		public void ShouldRemovePersonAbsenceFromRepository()
		{
			var absenceLayer = MockRepository.GenerateMock<IAbsenceLayer>();
			var start = new DateTime(2013, 6, 12, 5, 0, 0, DateTimeKind.Utc);
			absenceLayer.Stub(x => x.Period).Return(new DateTimePeriod(start, start.AddHours(2)));
			var personAbsence = new PersonAbsence(PersonFactory.CreatePersonWithId(), new FakeCurrentScenario().Current(), absenceLayer);
			personAbsence.SetId(new Guid());

			var personAbsenceRepository = new TestWriteSideRepository<IPersonAbsence>() { personAbsence };

			var target = new RemovePersonAbsenceCommandHandler(new FakeCurrentDatasource(), personAbsenceRepository);

			var command = new RemovePersonAbsenceCommand
				{
					PersonAbsenceId = personAbsence.Id.Value
				};

			target.Handle(command);

			Assert.That(personAbsenceRepository.Any() == false); 
		}

		[Test]
		public void ShouldRaisePersonAbsenceRemovedEvent()
		{
			var currentDataSource = new FakeCurrentDatasource("datasource");

			var absenceLayer = MockRepository.GenerateMock<IAbsenceLayer>();
			var start = new DateTime(2013, 6, 12, 5, 0, 0, DateTimeKind.Utc);
			absenceLayer.Stub(x => x.Period).Return(new DateTimePeriod(start, start.AddHours(2)));
			var personAbsence = new PersonAbsence(PersonFactory.CreatePersonWithId(), new FakeCurrentScenario().Current(), absenceLayer);
			personAbsence.SetId(new Guid());

			var personAbsenceRepository = new TestWriteSideRepository<IPersonAbsence>() { personAbsence };
			
			var target = new RemovePersonAbsenceCommandHandler(currentDataSource, personAbsenceRepository);

			var command = new RemovePersonAbsenceCommand
				{
					PersonAbsenceId = personAbsence.Id.Value
				};

			target.Handle(command);
			var @event = personAbsence.PopAllEvents().Single() as PersonAbsenceRemovedEvent;
			@event.Datasource.Should().Be("datasource");
			@event.BusinessUnitId.Should().Be(personAbsence.BusinessUnit.Id.Value);
			@event.PersonId.Should().Be(personAbsence.Person.Id.Value);
			@event.ScenarioId.Should().Be(personAbsence.Scenario.Id.Value);
			@event.StartDateTime.Should().Be(personAbsence.Layer.Period.StartDateTime.AddHours(-24));
			@event.EndDateTime.Should().Be(personAbsence.Layer.Period.EndDateTime);
			
		}
	}
	
}