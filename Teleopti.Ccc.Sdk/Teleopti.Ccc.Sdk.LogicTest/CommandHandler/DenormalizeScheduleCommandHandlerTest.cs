using System;
using System.Collections.Generic;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.ApplicationLayer;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.IocCommon;
using Teleopti.Ccc.Sdk.Common.DataTransferObject.Commands;
using Teleopti.Ccc.Sdk.Logic.CommandHandler;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.TestCommon.FakeRepositories;
using Teleopti.Ccc.TestCommon.IoC;

namespace Teleopti.Ccc.Sdk.LogicTest.CommandHandler
{
	[TestFixture]
	[DomainTest]
	public class DenormalizeScheduleCommandHandlerTest : IExtendSystem, IIsolateSystem
	{
		public DenormalizeScheduleCommandHandler Target;
		public FakeEventPopulatingPublisherForTest EventPopulatingPublisher;
		public FakePersonRepository PersonRepository;
		public FakeScenarioRepository ScenarioRepository;

		public void Extend(IExtend extend, IIocConfiguration configuration)
		{
			extend.AddService<DenormalizeScheduleCommandHandler>();
		}

		public void Isolate(IIsolate isolate)
		{
			isolate.UseTestDouble<FakeEventPopulatingPublisherForTest>().For<IEventPopulatingPublisher>();
		}

		[Test]
		public void ShouldPublishEvent()
		{
			var person = PersonFactory.CreatePerson().WithId();
			PersonRepository.Add(person);
			var scenario = ScenarioFactory.CreateScenarioWithId("d", true);
			ScenarioRepository.Add(scenario);
			var startDate = new DateTime(2018, 07, 24, 10, 0, 0);
			var endDate = new DateTime(2018, 07, 25, 10, 0, 0);
			var command = new DenormalizeScheduleCommandDto()
			{
				PersonId = person.Id.GetValueOrDefault(),
				ScenarioId = scenario.Id.GetValueOrDefault(),
				StartDateTime = startDate,
				EndDateTime = endDate
			};
			Target.Handle(command);
			command.Result.AffectedId.Should().Be.EqualTo(Guid.Empty);
			command.Result.AffectedItems.Should().Be(1);
			EventPopulatingPublisher.PublishedEvent.Count.Should().Be(1);
		}

		[Test]
		public void ShouldNotPublishIfPersonNotFound()
		{
			var person = PersonFactory.CreatePerson().WithId();
			var command = new DenormalizeScheduleCommandDto() { PersonId = person.Id.GetValueOrDefault() };
			Target.Handle(command);
			command.Result.AffectedId.Should().Be.EqualTo(Guid.Empty);
			command.Result.AffectedItems.Should().Be(0);
		}

		[Test]
		public void ShouldPublishEventIfPersonFound()
		{
			var person = PersonFactory.CreatePerson().WithId();
			PersonRepository.Add(person);
			var scenario = ScenarioFactory.CreateScenarioWithId("d", true);
			ScenarioRepository.Add(scenario);
			var startDate = new DateTime(2018, 07, 24, 10, 0, 0);
			var endDate = new DateTime(2018, 07, 25, 10, 0, 0);
			var command = new DenormalizeScheduleCommandDto()
			{
				PersonId = person.Id.GetValueOrDefault(),
				ScenarioId = scenario.Id.GetValueOrDefault(),
				StartDateTime = startDate,
				EndDateTime = endDate
			};
			Target.Handle(command);
			command.Result.AffectedId.Should().Be.EqualTo(Guid.Empty);
			command.Result.AffectedItems.Should().Be(1);
		}


		[Test]
		public void ShouldNotPublishEventIfScenarioIsInvalid()
		{
			var person = PersonFactory.CreatePerson().WithId();
			PersonRepository.Add(person);
			var command = new DenormalizeScheduleCommandDto() { PersonId = person.Id.GetValueOrDefault(), ScenarioId = Guid.NewGuid() };
			Target.Handle(command);
			command.Result.AffectedId.Should().Be.EqualTo(Guid.Empty);
			command.Result.AffectedItems.Should().Be(0);
		}

		[Test]
		public void ShouldPublishEventIfScenarioIsValid()
		{
			var person = PersonFactory.CreatePerson().WithId();
			PersonRepository.Add(person);
			var scenario = ScenarioFactory.CreateScenarioWithId("d", true);
			ScenarioRepository.Add(scenario);
			var startDate = new DateTime(2018, 07, 24, 10, 0, 0);
			var endDate = new DateTime(2018, 07, 25, 10, 0, 0);
			var command = new DenormalizeScheduleCommandDto()
			{
				PersonId = person.Id.GetValueOrDefault(),
				ScenarioId = scenario.Id.GetValueOrDefault(),
				StartDateTime = startDate,
				EndDateTime = endDate
			};
			Target.Handle(command);
			command.Result.AffectedId.Should().Be.EqualTo(Guid.Empty);
			command.Result.AffectedItems.Should().Be(1);
		}

		[Test]
		public void ShouldNotPublishIfDateIsEmpty()
		{
			var person = PersonFactory.CreatePerson().WithId();
			PersonRepository.Add(person);
			var scenario = ScenarioFactory.CreateScenarioWithId("d", true);
			ScenarioRepository.Add(scenario);
			var command = new DenormalizeScheduleCommandDto() { PersonId = person.Id.GetValueOrDefault(), ScenarioId = scenario.Id.GetValueOrDefault() };
			Target.Handle(command);
			command.Result.AffectedId.Should().Be.EqualTo(Guid.Empty);
			command.Result.AffectedItems.Should().Be(0);
		}

		[Test]
		public void ShouldNotPublishIfDatePeriodIsInvalid()
		{
			var person = PersonFactory.CreatePerson().WithId();
			PersonRepository.Add(person);
			var scenario = ScenarioFactory.CreateScenarioWithId("d", true);
			ScenarioRepository.Add(scenario);
			var startDate = new DateTime(2018, 07, 25, 10, 0, 0);
			var endDate = new DateTime(2018, 07, 24, 10, 0, 0);
			var command = new DenormalizeScheduleCommandDto()
			{
				PersonId = person.Id.GetValueOrDefault(),
				ScenarioId = scenario.Id.GetValueOrDefault(),
				StartDateTime = startDate,
				EndDateTime = endDate
			};
			Target.Handle(command);
			command.Result.AffectedId.Should().Be.EqualTo(Guid.Empty);
			command.Result.AffectedItems.Should().Be(0);
		}
	}


	public class FakeEventPopulatingPublisherForTest : IEventPopulatingPublisher
	{
		public List<IEvent> PublishedEvent = new List<IEvent>();

		public void Publish(params IEvent[] events)
		{
			PublishedEvent.AddRange(events);
		}
	}
}
