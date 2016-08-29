using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.ApplicationLayer;
using Teleopti.Ccc.Domain.ApplicationLayer.Rta.ReadModelUpdaters;
using Teleopti.Ccc.Domain.ApplicationLayer.Rta.Service;
using Teleopti.Ccc.Domain.FeatureFlags;
using Teleopti.Ccc.Domain.Helper;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.UnitOfWork;
using Teleopti.Ccc.IocCommon;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeRepositories.Rta;
using Teleopti.Ccc.TestCommon.IoC;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.InfrastructureTest.Rta
{
	[TestFixture]
	[MultiDatabaseTest]
	[Toggle(Toggles.RTA_RuleMappingOptimization_39812)]
	public class SnapshotTest : ISetup
	{
		public DatabaseLegacy Database;
		public WithAnalyticsUnitOfWork WithAnalyticsUnitOfWork;
		public WithUnitOfWork WithUnitOfWork;
		public IPersonRepository Persons;
		public Domain.ApplicationLayer.Rta.Service.Rta Rta;
		public IAgentStateReadModelReader ReadModels;
		public ConfigurableSyncEventPublisher Publisher;

		public void Setup(ISystem system, IIocConfiguration configuration)
		{
			system.UseTestDouble<ConfigurableSyncEventPublisher>().For<IEventPublisher>();
		}

		[Test]
		public void ShouldLogOutAgentsNotInSnapshot()
		{
			Publisher.AddHandler(typeof(PersonAssociationChangedEventPublisher));
			Publisher.AddHandler(typeof(AgentStateCleaner));
			Publisher.AddHandler(typeof(MappingReadModelUpdater));
			var logOutBySnapshot = Domain.ApplicationLayer.Rta.Service.Rta.LogOutBySnapshot;
			Database
				.WithDataSource("sourceId")
				.WithAgent("user1")
				.WithAgent("user2")

				.WithStateGroup("phone")
				.WithRule("InAdherence", Adherence.In)
				.WithMapping("phone", "InAdherence")

				.WithStateGroup(logOutBySnapshot)
				.WithRule("OutAdherence", Adherence.Out)
				.WithMapping(logOutBySnapshot, "OutAdherence")

				.PublishRecurringEvents()
				;
			var person = WithUnitOfWork.Get(() => Persons.LoadAll().Single(x => x.Name.FirstName == "user2"));
			Rta.SaveStateBatch(new BatchForTest
			{
				SnapshotId = "2016-04-07 08:00".Utc(),
				States = new[]
				{
					new BatchStateForTest
					{
						UserCode = "user1",
						StateCode = "phone"
					},
					new BatchStateForTest
					{
						UserCode = "user2",
						StateCode = "phone"
					}
				}
			});
			Rta.CloseSnapshot(new CloseSnapshotForTest
			{
				SnapshotId = "2016-04-07 08:00".Utc()
			});

			Rta.SaveStateBatch(new BatchForTest
			{
				SnapshotId = "2016-04-07 08:10".Utc(),
				States = new[]
				{
					new BatchStateForTest
					{
						UserCode = "user1",
						StateCode = "phone"
					}
				}
			});
			Rta.CloseSnapshot(new CloseSnapshotForTest
			{
				SnapshotId = "2016-04-07 08:10".Utc()
			});


			WithUnitOfWork.Get(() => ReadModels.Load(new[] { person.Id.Value }))
				.SingleOrDefault()
				.RuleName.Should().Be("OutAdherence");
		}
	}
}