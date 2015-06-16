using System;
using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Aop;
using Teleopti.Ccc.Domain.ApplicationLayer.Events;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.Assignment;
using Teleopti.Ccc.IocCommon;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.TestCommon.IoC;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.InfrastructureTest.UnitOfWork.MessageSenders
{
	[TestFixture]
	[PrincipalAndStateTest]
	public class EventsTest : ISetup
	{
		public void Setup(ISystem system, IIocConfiguration configuration)
		{
			system.AddService<TheServiceImpl>();
		}

		public TheServiceImpl TheService;
		public FakeEventPublisher EventsPublisher;

		public class TheServiceImpl
		{
			public readonly IScenarioRepository ScenarioRepository;
			public readonly IPersonRepository PersonRepository;
			public readonly IPersonAssignmentRepository PersonAssignmentRepository;
			public readonly IActivityRepository ActivityRepository;

			public TheServiceImpl(
				IScenarioRepository scenarioRepository, 
				IPersonRepository personRepository, 
				IPersonAssignmentRepository personAssignmentRepository,
				IActivityRepository activityRepository
				)
			{
				ScenarioRepository = scenarioRepository;
				PersonRepository = personRepository;
				PersonAssignmentRepository = personAssignmentRepository;
				ActivityRepository = activityRepository;
			}

			[UnitOfWork]
			public virtual void Does(Action<TheServiceImpl> action)
			{
				action(this);
			}

		}

		[Test]
		public void ShouldPublishEvents()
		{
			TheService.Does(service =>
			{
				var scenario = ScenarioFactory.CreateScenario(".", true, false);
				service.ScenarioRepository.Add(scenario);

				var person = PersonFactory.CreatePerson();
				service.PersonRepository.Add(person);

				var activity = new Activity(".");
				service.ActivityRepository.Add(activity);

				var personAssignment = new PersonAssignment(person, scenario, new DateOnly(2015, 6, 15));
				personAssignment.AddActivity(activity, new DateTimePeriod(2015, 6, 15, 8, 2015, 6, 15, 17));

				service.PersonAssignmentRepository.Add(personAssignment);

			});

			EventsPublisher.PublishedEvents.OfType<ActivityAddedEvent>()
				.Should().Have.Count.GreaterThan(0);
		}
	}
}
