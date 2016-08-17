using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.ApplicationLayer.Rta.Service;
using Teleopti.Ccc.Domain.Helper;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.UnitOfWork;
using Teleopti.Ccc.TestCommon.FakeRepositories.Rta;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.InfrastructureTest.Rta
{
	[TestFixture]
	[MultiDatabaseTest]
	public class SnapshotTest
	{
		public DatabaseLegacy Database;
		public WithAnalyticsUnitOfWork WithAnalyticsUnitOfWork;
		public WithUnitOfWork WithUnitOfWork;
		public IPersonRepository Persons;
		public Domain.ApplicationLayer.Rta.Service.Rta Rta;
		public IAgentStateReadModelReader ReadModels;

		[Test]
		public void ShouldLogOutAgentsNotInSnapshot()
		{
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