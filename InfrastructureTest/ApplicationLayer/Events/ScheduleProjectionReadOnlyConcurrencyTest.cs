using System.Linq;
using NHibernate.Util;
using NUnit.Framework;
using Teleopti.Ccc.Domain.ApplicationLayer;
using Teleopti.Ccc.Domain.ApplicationLayer.ScheduleChangedEventHandlers;
using Teleopti.Ccc.Domain.ApplicationLayer.ScheduleChangedEventHandlers.ScheduleProjection;
using Teleopti.Ccc.Domain.FeatureFlags;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.UnitOfWork;
using Teleopti.Ccc.IocCommon;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeRepositories;
using Teleopti.Ccc.TestCommon.IoC;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.InfrastructureTest.ApplicationLayer.Events
{
	[TestFixture]
	[DatabaseTest]
	public class ScheduleProjectionReadOnlyConcurrencyTest : ISetup
	{
		public void Setup(ISystem system, IIocConfiguration configuration)
		{
			system.UseTestDouble<ConfigurableAsyncEventPublisher>().For<IEventPublisher>();
			system.UseTestDouble<FakeRepositoryFactory>().For<IRepositoryFactory>();
			system.UseTestDouble<PersonAssignmentRepositoryWithRandomLatency>().For<IPersonAssignmentRepository>();
		}

		public ProjectionChangedEventPublisher Target;
		public Database Database;
		public ConfigurableAsyncEventPublisher Publisher;
		public IScheduleProjectionReadOnlyPersister Persister;
		public WithUnitOfWork UnitOfWork;

		[Test]
		[Explicit]
		public void ShouldHandleManyWorkers()
		{
			Publisher.AddHandler(typeof(ScheduleChangedEventPublisher));
			Publisher.AddHandler(typeof(ProjectionChangedEventPublisher));
			Publisher.AddHandler(typeof(ScheduleProjectionReadOnlyUpdater));
			var dates = Enumerable.Range(0, 100)
				.Select(n => "2016-05-02".Date().AddDays(n))
				.ToArray();
			Database
				.WithAgent()
				.WithActivity("Phone")
				.WithActivity("Admin")
				;
			
			dates.ForEach(d =>
			{
				var time = d.Date;
				Database
					.WithAssignment(d)
					.WithLayer("Phone", time.AddHours(8), time.AddHours(12))
					.WithLayer("Admin", time.AddHours(12), time.AddHours(17))
					;
			});
			Publisher.Wait();

			var personId = Database.CurrentPersonId();
			var scenarioId = Database.CurrentScenarioId();
			UnitOfWork.Do(() =>
			{
				dates.ForEach(d =>
				{
					var actual = Persister.ForPerson(d, personId, scenarioId);
					Assert.That(actual.Count(), Is.EqualTo(2), d + " " + personId + " " + scenarioId);
				});
			});
		}
	}
}