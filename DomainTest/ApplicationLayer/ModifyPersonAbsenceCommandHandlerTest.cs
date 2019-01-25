using System;
using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.ApplicationLayer.Commands;
using Teleopti.Ccc.Domain.ApplicationLayer.Events;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.IocCommon;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.TestCommon.FakeRepositories;
using Teleopti.Ccc.TestCommon.IoC;


namespace Teleopti.Ccc.DomainTest.ApplicationLayer
{
	[DomainTest]
	public class ModifyPersonAbsenceCommandHandlerTest : IExtendSystem, IIsolateSystem
	{
		public ModifyPersonAbsenceCommandHandler Target;
		public FakePersonAbsenceRepository PersonAbsenceRepository;
		public FakeScenarioRepository ScenarioRepository;

		[Test]
		public void ShowThrowExceptionIfPersonAbsenceDoesNotExist()
		{
			var currentScenario = ScenarioRepository.Has("Default");
			var personAbsence = new PersonAbsence(PersonFactory.CreatePersonWithId(), currentScenario,
				new AbsenceLayer(new Absence(), new DateTimePeriod(2013, 11, 27, 8, 2013, 11, 27, 16))).WithId();
			PersonAbsenceRepository.Add(personAbsence);
			
			var command = new ModifyPersonAbsenceCommand
			{
				PersonAbsenceId = Guid.NewGuid(),
				PersonId  = Guid.NewGuid(),
				StartTime = new DateTime(2013, 11, 27, 14, 00, 00, DateTimeKind.Utc),
				EndTime = new DateTime(2013, 11, 27, 15, 00, 00, DateTimeKind.Utc),
				TrackedCommandInfo = new TrackedCommandInfo
				{
					OperatedPersonId = Guid.NewGuid()
				}
			};
			var ex = Assert.Throws<InvalidOperationException>(() => Target.Handle(command)).ToString();
			ex.Should().Contain(command.PersonAbsenceId.ToString());
			ex.Should().Contain(command.StartTime.ToString());
			ex.Should().Contain(command.EndTime.ToString());
		}
		
		[Test]
		public void ShouldRaiseModifyPersonAbsenceEvent()
		{
			var currentScenario = ScenarioRepository.Has("Default");
			var originalDateTimePeriod = new DateTimePeriod (2013, 11, 27, 8, 2013, 11, 27, 16);
			var personAbsence = PersonAbsenceFactory.CreatePersonAbsence(PersonFactory.CreatePersonWithId(),
				currentScenario, originalDateTimePeriod).WithId();
			PersonAbsenceRepository.Add(personAbsence);
			
			var command = new ModifyPersonAbsenceCommand
			{
				PersonAbsenceId = personAbsence.Id.Value,
				PersonId = Guid.NewGuid(),
				StartTime = new DateTime(2013, 11, 27, 8, 00, 00, DateTimeKind.Utc),
				EndTime = new DateTime(2013, 11, 27, 15, 00, 00, DateTimeKind.Utc),
				TrackedCommandInfo = new TrackedCommandInfo
				{
					OperatedPersonId = Guid.NewGuid()

				}
			};

			Target.Handle(command);

			var @event = personAbsence.PopAllEvents(null).Single() as PersonAbsenceModifiedEvent;
			@event.AbsenceId.Should().Be(personAbsence.Id.Value);
			@event.PersonId.Should().Be(personAbsence.Person.Id.Value);
			@event.ScenarioId.Should().Be(currentScenario.Id.Value);
			@event.StartDateTime.Should().Be(command.StartTime);
			@event.EndDateTime.Should().Be(originalDateTimePeriod.EndDateTime);
		}

		[Test]
		public void ShouldReduceEndTimeWhenBackToWork()
		{
			var currentScenario = ScenarioRepository.Has("Default");
			var personAbsence = PersonAbsenceFactory.CreatePersonAbsence(PersonFactory.CreatePersonWithId(),
				currentScenario, new DateTimePeriod(2013, 11, 27, 11, 2013, 11, 27, 16)).WithId();
			PersonAbsenceRepository.Has(personAbsence);

			var command = new ModifyPersonAbsenceCommand
			{
				PersonAbsenceId = personAbsence.Id.Value,
				StartTime = new DateTime(2013, 11, 27, 11, 00, 00, DateTimeKind.Utc),
				EndTime = new DateTime(2013, 11, 27, 15, 00, 00, DateTimeKind.Utc),
				TrackedCommandInfo = new TrackedCommandInfo
				{
					OperatedPersonId = Guid.NewGuid()
				}
			};

			Target.Handle(command);
			
			personAbsence.Layer.Period.StartDateTime.Should().Be(command.StartTime);
			personAbsence.Layer.Period.EndDateTime.Should().Be(command.EndTime);

		}

		public void Extend(IExtend extend, IocConfiguration configuration)
		{
			extend.AddService<ModifyPersonAbsenceCommandHandler>();
		}

		public void Isolate(IIsolate isolate)
		{
			isolate.UseTestDouble<FakePersonAbsenceRepository>().For<IWriteSideRepositoryTypedId<IPersonAbsence,Guid>>();
		}
	}
}
