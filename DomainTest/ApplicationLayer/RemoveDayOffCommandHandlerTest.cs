using NUnit.Framework;
using SharpTestsEx;
using System;
using System.Linq;
using Teleopti.Ccc.Domain.ApplicationLayer;
using Teleopti.Ccc.Domain.ApplicationLayer.Commands;
using Teleopti.Ccc.Domain.ApplicationLayer.Events;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.IocCommon;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.TestCommon.FakeRepositories;
using Teleopti.Ccc.TestCommon.IoC;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.ApplicationLayer
{
	[DomainTest]
	public class RemoveDayOffCommandHandlerTest : ISetup
	{
		public RemoveDayOffCommandHandler Target;
		public FakePersonRepository PersonRepository;
		public FakeScenarioRepository ScenarioRepository;
		public FakeDayOffTemplateRepository DayOffTemplateRepository;
		public FakePersonAssignmentRepository PersonAssignmentRepository;
		public IScheduleStorage ScheduleStorage;
		public void Setup(ISystem system, IIocConfiguration configuration)
		{
			system.UseTestDouble<RemoveDayOffCommandHandler>().For<IHandleCommand<RemoveDayOffCommand>>();
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
		[Test, Ignore("TODO")]
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

			var theEvent = personAssignment.PopAllEvents().OfType<DayOffAddedEvent>().Single();
			theEvent.Date.Should().Be.EqualTo(date.Date);
			theEvent.PersonId.Should().Be.EqualTo(person.Id.Value);
			theEvent.CommandId.Should().Be.EqualTo(trackId);
			theEvent.ScenarioId.Should().Be.EqualTo(scenario.Id.Value);
		}

	}
}
