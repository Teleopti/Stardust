using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.ApplicationLayer;
using Teleopti.Ccc.Domain.ApplicationLayer.Events;
using Teleopti.Ccc.Domain.ApplicationLayer.Rta.Service;
using Teleopti.Ccc.Domain.ApplicationLayer.ScheduleChangedEventHandlers;
using Teleopti.Ccc.Domain.ApplicationLayer.ScheduleChangedEventHandlers.ScheduleProjection;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Common.Time;
using Teleopti.Ccc.Domain.FeatureFlags;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.UnitOfWork;
using Teleopti.Ccc.IocCommon;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.IoC;
using ExternalLogon = Teleopti.Ccc.Domain.ApplicationLayer.Rta.Service.ExternalLogon;

namespace Teleopti.Ccc.InfrastructureTest.Rta
{
	[TestFixture]
	[MultiDatabaseTest]
	[Toggle(Toggles.RTA_ScheduleQueryOptimization_40260)]
	public class ScheduleInvalidationTest : ISetup
	{
		public Database Database;
		public AnalyticsDatabase Analytics;
		public WithAnalyticsUnitOfWork WithAnalyticsUnitOfWork;
		public WithUnitOfWork WithUnitOfWork;
		public IPersonRepository Persons;
		public IAgentStatePersister AgentStatePersister;
		public Domain.ApplicationLayer.Rta.Service.Rta Rta;
		public ConfigurableSyncEventPublisher Publisher;
		public MutableNow Now;
		public ICurrentDataSource DataSource;

		public void Setup(ISystem system, IIocConfiguration configuration)
		{
			system.UseTestDouble<ConfigurableSyncEventPublisher>().For<IEventPublisher>();
			system.AddService<ScheduleProjectionReadOnlyUpdater>();
			system.AddService<ProjectionChangedEventPublisher>();
		}

		[Test]
		public void ShouldInvalidateSchedules()
		{
			Publisher.AddHandler(typeof(ScheduleProjectionReadOnlyUpdater));
			Publisher.AddHandler(typeof(AgentStateMaintainer));
			Now.Is("2016-09-06 15:30");
			Analytics.WithDataSource(9, "sourceId");
			Database
				.WithAgent("user")
				.WithStateGroup("phone")
				.WithStateCode("phone")
				.PublishRecurringEvents()
				;

			var person = Database.CurrentPersonId();

			WithUnitOfWork.Do(() =>
			{
				AgentStatePersister.Prepare(new AgentStatePrepare
				{
					PersonId = person,
					ExternalLogons = new[]
					{
						new ExternalLogon
						{
							DataSourceId = 9,
							UserCode = "user"
						}
					}
				}, DeadLockVictim.Yes);
				AgentStatePersister.Update(new AgentStateForUpsert
				{
					PersonId = person,
					Schedule = new[]
					{
						new ScheduledActivity {Name = "phone"}
					}
				});
			});
			Publisher.Publish(new ProjectionChangedEvent
			{
				PersonId = person,
				ScheduleDays = new ProjectionChangedEventScheduleDay[] { }
			});

			WithUnitOfWork.Get(() => AgentStatePersister.Get(person))
				.Schedule.Should().Be.Null();
		}
	}
}