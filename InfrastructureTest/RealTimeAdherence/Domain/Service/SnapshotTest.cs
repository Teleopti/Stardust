﻿using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.ApplicationLayer;
using Teleopti.Ccc.Domain.Helper;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.RealTimeAdherence.ApplicationLayer.ReadModels;
using Teleopti.Ccc.Domain.RealTimeAdherence.Domain.Service;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.UnitOfWork;
using Teleopti.Ccc.IocCommon;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeRepositories.Rta;
using Teleopti.Ccc.TestCommon.IoC;

namespace Teleopti.Ccc.InfrastructureTest.RealTimeAdherence.Domain.Service
{
	[TestFixture]
	[MultiDatabaseTest]
	public class SnapshotTest : IIsolateSystem
	{
		public Database Database;
		public AnalyticsDatabase Analytics;
		public WithAnalyticsUnitOfWork WithAnalyticsUnitOfWork;
		public WithUnitOfWork WithUnitOfWork;
		public IPersonRepository Persons;
		public Ccc.Domain.RealTimeAdherence.Domain.Service.Rta Rta;
		public IAgentStateReadModelReader ReadModels;
		public FakeEventPublisher Publisher;

		public void Isolate(IIsolate isolate)
		{
			isolate.UseTestDouble<FakeEventPublisher>().For<IEventPublisher>();
		}

		[Test]
		public void ShouldLogOutAgentsNotInSnapshot()
		{
			Publisher.AddHandler(typeof(PersonAssociationChangedEventPublisher));
			Publisher.AddHandler(typeof(AgentStateMaintainer));
			Publisher.AddHandler(typeof(MappingReadModelUpdater));
			Publisher.AddHandler(typeof(CurrentScheduleReadModelUpdater));
			Publisher.AddHandler(typeof(ExternalLogonReadModelUpdater));
			Publisher.AddHandler(typeof(AgentStateReadModelMaintainer));
			Publisher.AddHandler(typeof(AgentStateReadModelUpdater));
			var logOutBySnapshot = Ccc.Domain.RealTimeAdherence.Domain.Service.Rta.LogOutBySnapshot;
			Analytics.WithDataSource(9, "sourceId");
			Database
				.WithAgent("user1")
				.WithAgent("user2")
				
				.WithStateGroup("phone")
				.WithStateCode("phone")
				.WithRule("InAdherence", 0, Adherence.In)
				.WithMapping("phone", "InAdherence")

				.WithStateGroup(logOutBySnapshot, true, true)
				.WithStateCode(logOutBySnapshot)
				.WithRule("OutAdherence", -1, Adherence.Out)
				.WithMapping(logOutBySnapshot, "OutAdherence")

				.PublishRecurringEvents()
				;
			var person = WithUnitOfWork.Get(() => Persons.LoadAll().Single(x => x.Name.FirstName == "user2"));
			Rta.Process(new BatchForTest
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

			Rta.Process(new BatchForTest
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


			WithUnitOfWork.Get(() => ReadModels.Read(new[] { person.Id.Value }))
				.SingleOrDefault()
				.RuleName.Should().Be("OutAdherence");
		}
	}
}