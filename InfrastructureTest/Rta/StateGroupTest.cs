using System;
using System.Data.SqlClient;
using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.RealTimeAdherence;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Ccc.Infrastructure.Rta;
using Teleopti.Ccc.InfrastructureTest.Helper;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeData;

namespace Teleopti.Ccc.InfrastructureTest.Rta
{
	[TestFixture]
	public class StateGroupTest : DatabaseTestWithoutTransaction
	{

		[Test]
		public void ShouldAddStateCodeToDefaultStateGroup()
		{
			using (new DefaultStateGroupCreator(PersistAndRemoveFromUnitOfWork))
			{
				var target = new DatabaseWriter(new DatabaseConnectionFactory(), new FakeDatabaseConnectionStringHandler());

				var info = target.AddAndGetStateCode("phone", null, Guid.Empty, BusinessUnitFactory.BusinessUnitUsedInTest.Id.Value);

				new RtaStateGroupRepository(UnitOfWork).Get(info.StateGroupId)
					.StateCollection.Single()
					.StateCode
					.Should().Be.EqualTo("phone");
			}
		}

		[Test]
		public void ShouldNotAddEmptyStateCode()
		{
			using (new DefaultStateGroupCreator(PersistAndRemoveFromUnitOfWork))
			{
				var target = new DatabaseWriter(new DatabaseConnectionFactory(), new FakeDatabaseConnectionStringHandler());

				var info = target.AddAndGetStateCode("", null, Guid.Empty, BusinessUnitFactory.BusinessUnitUsedInTest.Id.Value);

				new RtaStateGroupRepository(UnitOfWork).Get(info.StateGroupId)
					.StateCollection
					.Should().Be.Empty();
			}

		}

		[Test]
		public void ShouldAddStateCodeWithDescription()
		{
			using (new DefaultStateGroupCreator(PersistAndRemoveFromUnitOfWork))
			{
				var target = new DatabaseWriter(new DatabaseConnectionFactory(), new FakeDatabaseConnectionStringHandler());

				var info = target.AddAndGetStateCode(" ", "talk alot of blah blah", Guid.Empty, BusinessUnitFactory.BusinessUnitUsedInTest.Id.Value);

				new RtaStateGroupRepository(UnitOfWork).Get(info.StateGroupId)
					.StateCollection.Single()
					.Name
					.Should().Be.EqualTo("talk alot of blah blah");
			}
		}

		[Test]
		public void ShouldGetStateCodeWithDescription()
		{
			using (new DefaultStateGroupCreator(PersistAndRemoveFromUnitOfWork))
			{
				var target = new DatabaseWriter(new DatabaseConnectionFactory(), new FakeDatabaseConnectionStringHandler());

				var info = target.AddAndGetStateCode(" ", "talk alot of blah blah", Guid.Empty, BusinessUnitFactory.BusinessUnitUsedInTest.Id.Value);

				info.StateName.Should().Be.EqualTo("talk alot of blah blah");
			}
		}
	}

	public class DefaultStateGroupCreator : IDisposable
	{
		public DefaultStateGroupCreator(Action<RtaStateGroup> persistAction)
		{
			var stateGroup = new RtaStateGroup("default", true, true);
			persistAction(stateGroup);
		}

		public void Dispose()
		{
			applySql("DELETE FROM dbo.RtaState");
			applySql("DELETE FROM dbo.RtaStateGroup");
		}

		private static void applySql(string sql)
		{
			using (var connection = new SqlConnection(ConnectionStringHelper.ConnectionStringUsedInTests))
			{
				connection.Open();
				using (var command = new SqlCommand(sql, connection))
				{
					command.ExecuteNonQuery();
				}
			}
		}
	}
}