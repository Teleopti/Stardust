using NUnit.Framework;
using SharpTestsEx;
using System;
using System.Linq;
using Teleopti.Ccc.Domain.ApplicationLayer;
using Teleopti.Ccc.Domain.ApplicationLayer.Commands;
using Teleopti.Ccc.Domain.ApplicationLayer.Events;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.IocCommon;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.TestCommon.FakeRepositories;
using Teleopti.Ccc.TestCommon.IoC;


namespace Teleopti.Ccc.DomainTest.ApplicationLayer
{
	[DomainTest]
	public class AddDayOffCommandHandlerTest : IIsolateSystem
	{
		public AddDayOffCommandHandler Target;
		public FakePersonRepository PersonRepository;
		public FakeScenarioRepository ScenarioRepository;
		public FakeDayOffTemplateRepository DayOffTemplateRepository;
		public FakePersonAssignmentRepository PersonAssignmentRepository;
		public IScheduleStorage ScheduleStorage;
		public void Isolate(IIsolate isolate)
		{
			isolate.UseTestDouble<AddDayOffCommandHandler>().For<IHandleCommand<AddDayOffCommand>>();
		}

		[Test]
		public void ShouldRaiseEventWhenDayOffAdded()
		{
			var date = new DateOnly(2018, 1, 11);
			var person = PersonFactory.CreatePersonWithId();
			PersonRepository.Has(person);

			var scenario = ScenarioFactory.CreateScenarioWithId("default", true);
			ScenarioRepository.Has(scenario);

			var dayOffTemplate = DayOffFactory.CreateDayOff(new Description("template")).WithId();
			DayOffTemplateRepository.Has(dayOffTemplate);


			var trackId = Guid.NewGuid();
			var command = new AddDayOffCommand
			{
				Person = person,
				StartDate = date,
				EndDate = date,
				Template = dayOffTemplate,
				TrackedCommandInfo = new TrackedCommandInfo
				{
					OperatedPersonId = Guid.NewGuid(),
					TrackId = trackId
				}
			};
			Target.Handle(command);

			var personAssignment = PersonAssignmentRepository.LoadAll().Single(p => p.Person.Id == person.Id.Value);
			personAssignment.Date.Should().Be.EqualTo(date);
			personAssignment.DayOff().DayOffTemplateId.Should().Be.EqualTo(dayOffTemplate.Id.Value);

			var theEvent = personAssignment.PopAllEvents().OfType<DayOffAddedEvent>().Single();
			theEvent.Date.Should().Be.EqualTo(date.Date);
			theEvent.PersonId.Should().Be.EqualTo(person.Id.Value);
			theEvent.CommandId.Should().Be.EqualTo(trackId);
			theEvent.ScenarioId.Should().Be.EqualTo(scenario.Id.Value);
		}

		[Test]
		public void ShouldRaiseEventPerDateWhenDayOffAddedForMultipleDays()
		{
			var dateOnlyPeriod = new DateOnlyPeriod(2018, 1, 11, 2018, 1, 15);
			var person = PersonFactory.CreatePersonWithId();
			PersonRepository.Has(person);

			var scenario = ScenarioFactory.CreateScenarioWithId("default", true);
			ScenarioRepository.Has(scenario);

			var dayOffTemplate = DayOffFactory.CreateDayOff(new Description("template")).WithId();
			DayOffTemplateRepository.Has(dayOffTemplate);


			var trackId = Guid.NewGuid();
			var command = new AddDayOffCommand
			{
				Person = person,
				StartDate = dateOnlyPeriod.StartDate,
				EndDate = dateOnlyPeriod.EndDate,
				Template = dayOffTemplate,
				TrackedCommandInfo = new TrackedCommandInfo
				{
					OperatedPersonId = Guid.NewGuid(),
					TrackId = trackId
				}
			};
			Target.Handle(command);

			var personAssignments = PersonAssignmentRepository.LoadAll()
									.Where(p => p.Person.Id == person.Id.Value)
									.ToArray();
			personAssignments.Length.Should().Be.EqualTo(5);

			var days = dateOnlyPeriod.DayCollection();
			foreach (var personAssignment in personAssignments)
			{
				var index = Array.IndexOf(personAssignments, personAssignment);
				var date = days[index].Date;
				personAssignment.Date.Date.Should().Be.EqualTo(date);
				personAssignment.DayOff().DayOffTemplateId.Should().Be.EqualTo(dayOffTemplate.Id.Value);

				var theEvent = personAssignment.PopAllEvents().OfType<DayOffAddedEvent>().Single();
				theEvent.Date.Should().Be.EqualTo(date);
				theEvent.PersonId.Should().Be.EqualTo(person.Id.Value);
				theEvent.CommandId.Should().Be.EqualTo(trackId);
				theEvent.ScenarioId.Should().Be.EqualTo(scenario.Id.Value);
			}
		}

		[Test]
		public void ShouldDayOffTempateBeTheNewAddedOneWhenAddingDayOffOnADayOffDay() {
			var date = new DateOnly(2018, 1, 11);
			var person = PersonFactory.CreatePersonWithId();
			PersonRepository.Has(person);

			var scenario = ScenarioFactory.CreateScenarioWithId("default", true);
			ScenarioRepository.Has(scenario);

			
			var dayOffTemplate = DayOffFactory.CreateDayOff(new Description("template")).WithId();
			DayOffTemplateRepository.Has(dayOffTemplate);
			var anotherDayOffTemplate = DayOffFactory.CreateDayOff(new Description("another template")).WithId();
			DayOffTemplateRepository.Has(anotherDayOffTemplate);
			var trackId = Guid.NewGuid();
			var command = new AddDayOffCommand
			{
				Person = person,
				StartDate = date,
				EndDate = date,
				Template = anotherDayOffTemplate,
				TrackedCommandInfo = new TrackedCommandInfo
				{
					OperatedPersonId = Guid.NewGuid(),
					TrackId = trackId
				}
			};
			Target.Handle(command);

			var personAssignment = PersonAssignmentRepository.LoadAll().Single(p => p.Person.Id == person.Id.Value);
			personAssignment.Date.Should().Be.EqualTo(date);
			personAssignment.DayOff().DayOffTemplateId.Should().Be.EqualTo(anotherDayOffTemplate.Id.Value);
		}
	}
}
