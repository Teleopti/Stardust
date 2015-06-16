﻿using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.ApplicationLayer.Events;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.Assignment;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.InfrastructureTest.UnitOfWork.MessageSenders
{
	[TestFixture]
	[PrincipalAndStateTest]
	public class EventsTest
	{
		public ICurrentUnitOfWorkFactory Uow;
		public FakeEventPublisher EventsPublisher;
		public IScenarioRepository ScenarioRepository;
		public IPersonRepository PersonRepository;
		public IPersonAssignmentRepository PersonAssignmentRepository;
		public IActivityRepository ActivityRepository;

		[Test]
		public void ShouldPublishEvents()
		{
			using (var uow = Uow.LoggedOnUnitOfWorkFactory().CreateAndOpenUnitOfWork())
			{
				var scenario = ScenarioFactory.CreateScenario(".", true, false);
				ScenarioRepository.Add(scenario);

				var person = PersonFactory.CreatePerson();
				PersonRepository.Add(person);

				var activity = new Activity(".");
				ActivityRepository.Add(activity);

				var personAssignment = new PersonAssignment(person, scenario, new DateOnly(2015, 6, 15));
				personAssignment.AddActivity(activity, new DateTimePeriod(2015, 6, 15, 8, 2015, 6, 15, 17));

				PersonAssignmentRepository.Add(personAssignment);

				uow.PersistAll();
			}

			EventsPublisher.PublishedEvents.OfType<ActivityAddedEvent>()
				.Should().Have.Count.GreaterThan(0);
		}
	}
}
