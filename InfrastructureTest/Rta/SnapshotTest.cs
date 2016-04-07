using System.Linq;
using NUnit.Framework;
using Rhino.Mocks;
using SharpTestsEx;
using Teleopti.Ccc.Domain.ApplicationLayer.Rta;
using Teleopti.Ccc.Domain.ApplicationLayer.Rta.Service;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.Helper;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.UnitOfWork;
using Teleopti.Ccc.IocCommon;
using Teleopti.Ccc.TestCommon.FakeRepositories;
using Teleopti.Ccc.TestCommon.FakeRepositories.Rta;
using Teleopti.Ccc.TestCommon.IoC;
using Teleopti.Ccc.TestCommon.TestData;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.InfrastructureTest.Rta
{
	[TestFixture]
	[MultiDatabaseTest]
	public class SnapshotTest : ISetup
	{
		public DatabaseManager Database;
		public WithAnalyticsUnitOfWork WithAnalyticsUnitOfWork;
		public WithUnitOfWork WithUnitOfWork;
		public IPersonRepository Persons;
		public Domain.ApplicationLayer.Rta.Service.Rta Rta;
		public IAgentStateReadModelPersister ReadModels;

		public void Setup(ISystem system, IIocConfiguration configuration)
		{
			system.AddService<DatabaseManager>();
		}

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
			Rta.SaveStateBatch(new[]
			{
				new ExternalUserStateForTest
				{
					UserCode = "user1",
					StateCode = "phone",
					IsSnapshot = true,
					BatchId = "2016-04-07 08:00".Utc()
				}, 
				new ExternalUserStateForTest
				{
					UserCode = "user2",
					StateCode = "phone",
					IsSnapshot = true,
					BatchId = "2016-04-07 08:00".Utc()
				},
				new ExternalUserStateForTest
				{
					UserCode = "",
					StateCode = "",
					IsSnapshot = true,
					BatchId = "2016-04-07 08:00".Utc()
				}
			});

			Rta.SaveStateBatch(new[]
			{
				new ExternalUserStateForTest
				{
					UserCode = "user1",
					StateCode = "phone",
					IsSnapshot = true,
					BatchId = "2016-04-07 08:10".Utc()
				},
				new ExternalUserStateForTest
				{
					UserCode = "",
					StateCode = "",
					IsSnapshot = true,
					BatchId = "2016-04-07 08:10".Utc()
				}
			});

			WithAnalyticsUnitOfWork.Get(() => ReadModels.Get(person.Id.Value))
				.RuleName.Should().Be("OutAdherence");
		}
	}
}