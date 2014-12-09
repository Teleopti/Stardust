using System;
using System.Data.SqlClient;
using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Ccc.Infrastructure.Rta;
using Teleopti.Ccc.Infrastructure.UnitOfWork;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.InfrastructureTest.Rta
{
	[TestFixture]
	[StateGroupTest]
	public class StateGroupTest 
	{
		[Test, Ignore]
		public void ShouldAddStateCodeToDefaultStateGroup()
		{
			using (var uow = UnitOfWorkFactory.Current.CreateAndOpenUnitOfWork())
			{
				var target = new DatabaseWriter(new DatabaseConnectionFactory(), new FakeDatabaseConnectionStringHandler());
				
				var defaultStateGroup = target.AddAndGetNewRtaState("phone", Guid.Empty, getBusinessUnitId(uow));

				new RtaStateGroupRepository(uow).Get(defaultStateGroup.StateGroupId)
					.StateCollection.Single()
					.Name
					.Should().Be.EqualTo("phone");
			}
		}

		[Test, Ignore]
		public void ShouldNotAddEmptyStateCode()
		{
			using (var uow = UnitOfWorkFactory.Current.CreateAndOpenUnitOfWork())
			{
				var target = new DatabaseWriter(new DatabaseConnectionFactory(), new FakeDatabaseConnectionStringHandler());

				var defaultStateGroup = target.AddAndGetNewRtaState("", Guid.Empty, getBusinessUnitId(uow));

				new RtaStateGroupRepository(uow).Get(defaultStateGroup.StateGroupId)
					.StateCollection
					.Should().Be.Empty();
			}
		}

		private static Guid getBusinessUnitId(IUnitOfWork uow)
		{
			return new BusinessUnitRepository(uow).LoadAll().Single().Id.Value;
		}
	}
	

	public class StateGroupTestAttribute : Attribute, ITestAction
	{
		private BusinessUnit _businessUnit;

		public ActionTargets Targets { get { return ActionTargets.Test; } }

		public void BeforeTest(TestDetails testDetails)
		{
			using (var uow = UnitOfWorkFactory.Current.CreateAndOpenUnitOfWork())
			{
				new BusinessUnitRepository(uow).Add(_businessUnit = BusinessUnitFactory.CreateSimpleBusinessUnit());
				new RtaStateGroupRepository(uow).Add(StateGroupFactory.CreateDefaultStateGroup(_businessUnit));
				
				uow.PersistAll();
			}
		}

		public void AfterTest(TestDetails testDetails)
		{
			applySql("DELETE FROM dbo.RtaState");
			applySql("DELETE FROM dbo.RtaStateGroup");
			applySql(string.Format("DELETE FROM dbo.BusinessUnit WHERE Id = '{0}'", _businessUnit.Id.Value));
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