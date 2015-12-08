using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.ApplicationLayer;
using Teleopti.Ccc.Domain.ApplicationLayer.Events;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.Assignment;
using Teleopti.Ccc.IocCommon;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.TestCommon.IoC;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.InfrastructureTest.UnitOfWork.PersistCallbacks
{
	[TestFixture]
	[PrincipalAndStateTest]
	public class FatClientOptimizationEventPublishingTest : ISetup, ISetupConfiguration
	{
		public ICurrentUnitOfWorkFactory Uow;
		public FakeEventPublisher EventsPublisher;
		public IScenarioRepository ScenarioRepository;
		public IPersonRepository PersonRepository;
		public IPersonAssignmentRepository PersonAssignmentRepository;
		public IActivityRepository ActivityRepository;

		public void SetupConfiguration(IocArgs args)
		{
			args.OptimizeScheduleChangedEvents = true;
		}

		public void Setup(ISystem system, IIocConfiguration configuration)
		{
			system.UseTestDouble<FakeEventPublisher>().For<IEventPublisher>();
		}
		
		[Test]
		public void ShouldOnlyPublishScheduleChangedEventWhenPersistingPersonAssignment()
		{
			var scenario = ScenarioFactory.CreateScenario(".", true, false);
			var person = PersonFactory.CreatePerson();
			var activity = new Activity(".");
			using (var uow = Uow.Current().CreateAndOpenUnitOfWork())
			{
				ScenarioRepository.Add(scenario);
				PersonRepository.Add(person);
				ActivityRepository.Add(activity);
				uow.PersistAll();
			}
			EventsPublisher.Clear();

			var personAssignment = new PersonAssignment(person, scenario, new DateOnly(2015, 6, 15));
			personAssignment.AddActivity(activity, new DateTimePeriod(2015, 6, 15, 8, 2015, 6, 15, 17));

			using (var uow = Uow.Current().CreateAndOpenUnitOfWork())
			{
				PersonAssignmentRepository.Add(personAssignment);
				uow.PersistAll();
			}

			EventsPublisher.PublishedEvents.All(x => x.GetType() == typeof(ScheduleChangedEvent))
				.Should().Be.True();
		}
	}
}