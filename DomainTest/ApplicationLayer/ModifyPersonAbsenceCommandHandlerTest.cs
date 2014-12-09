﻿using System;
using System.Linq;
using NUnit.Framework;
using Rhino.Mocks;
using SharpTestsEx;
using Teleopti.Ccc.Domain.ApplicationLayer.Commands;
using Teleopti.Ccc.Domain.ApplicationLayer.Events;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Security.Authentication;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.ApplicationLayer
{
	[TestFixture]
	public class ModifyPersonAbsenceCommandHandlerTest
	{

		[Test]
		public void ShowThrowExceptionIfPersonAbsenceDoesNotExist()
		{

			var currentScenario = new FakeCurrentScenario();
			var personAbsence = new PersonAbsence(PersonFactory.CreatePersonWithId(), currentScenario.Current(), MockRepository.GenerateMock<IAbsenceLayer>());
			var personAbsenceRepository = new FakePersonAbsenceWriteSideRepository() { personAbsence } ;
			var target = new ModifyPersonAbsenceCommandHandler(personAbsenceRepository, new UtcTimeZone());

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
			var ex = Assert.Throws<InvalidOperationException>(() => target.Handle(command)).ToString();
			ex.Should().Contain(command.PersonAbsenceId.ToString());
			ex.Should().Contain(command.StartTime.ToString());
			ex.Should().Contain(command.EndTime.ToString());
			
		}


		[Test]
		public void ShouldRaiseModifyPersonAbsenceEvent()
		{
			var currentScenario = new FakeCurrentScenario();
			var personAbsenceRepository = new FakePersonAbsenceWriteSideRepository()
				{
					PersonAbsenceFactory.CreatePersonAbsence (PersonFactory.CreatePersonWithId(), currentScenario.Current(),
						new DateTimePeriod(2013, 11, 27, 8, 2013, 11, 27, 16))
				};

			var target = new ModifyPersonAbsenceCommandHandler(personAbsenceRepository, new UtcTimeZone());
			var command = new ModifyPersonAbsenceCommand
			{
				PersonAbsenceId = personAbsenceRepository.Single().Id.Value,
				PersonId = Guid.NewGuid(),
				StartTime = new DateTime(2013, 11, 27, 8, 00, 00, DateTimeKind.Utc),
				EndTime = new DateTime(2013, 11, 27, 15, 00, 00, DateTimeKind.Utc),
				TrackedCommandInfo = new TrackedCommandInfo
				{
					OperatedPersonId = Guid.NewGuid()

				}
			};

			target.Handle(command);

			var @event = personAbsenceRepository.Single().PopAllEvents().Single() as PersonAbsenceModifiedEvent;
			@event.AbsenceId.Should().Be(personAbsenceRepository.Single().Id.Value);
			@event.PersonId.Should().Be(personAbsenceRepository.Single().Person.Id.Value);
			@event.ScenarioId.Should().Be(currentScenario.Current().Id.Value);
			@event.StartDateTime.Should().Be(command.StartTime);
			@event.EndDateTime.Should().Be(command.EndTime);
		}

		[Test]
		public void ShouldReduceEndTimeWhenBackToWork()
		{

			var currentScenario = new FakeCurrentScenario();
			var personAbsenceRepository = new FakePersonAbsenceWriteSideRepository()
				{
					PersonAbsenceFactory.CreatePersonAbsence (PersonFactory.CreatePersonWithId(), currentScenario.Current(),
						new DateTimePeriod(2013, 11, 27, 11, 2013, 11, 27, 16))
				};var target = new ModifyPersonAbsenceCommandHandler(personAbsenceRepository, new UtcTimeZone());

			var command = new ModifyPersonAbsenceCommand
			{
				PersonAbsenceId = personAbsenceRepository.Single().Id.Value,
				StartTime = new DateTime(2013, 11, 27, 11, 00, 00, DateTimeKind.Utc),
				EndTime = new DateTime(2013, 11, 27, 15, 00, 00, DateTimeKind.Utc),
				TrackedCommandInfo = new TrackedCommandInfo
				{
					OperatedPersonId = Guid.NewGuid()
				}
			};

			target.Handle(command);

			var absence = personAbsenceRepository.Single();

			absence.Layer.Period.StartDateTime.Should().Be(command.StartTime);
			absence.Layer.Period.EndDateTime.Should().Be(command.EndTime);

		}
	}
}
