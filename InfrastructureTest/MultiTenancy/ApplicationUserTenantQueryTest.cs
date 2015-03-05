using System;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Infrastructure.MultiTenancy;
using Teleopti.Ccc.Infrastructure.MultiTenancy.NHibernate;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Ccc.Infrastructure.UnitOfWork;
using Teleopti.Ccc.InfrastructureTest.Helper;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeData;

namespace Teleopti.Ccc.InfrastructureTest.MultiTenancy
{
	public class ApplicationUserTenantQueryTest : DatabaseTestWithoutTransaction
	{
		private Guid personId;
		private string correctUserName;
		private IApplicationUserTenantQuery target;
		private TenantUnitOfWorkManager _tenantUnitOfWorkManager;

		[Test]
		public void ShouldFindPersonId()
		{
			var result = target.Find(correctUserName);
			result.Id.Should().Be.EqualTo(personId);
		}

		[Test]
		public void ShouldFindTenant()
		{
			var result = target.Find(correctUserName);
			result.Tenant.Should().Be.EqualTo(Tenant.DefaultName);
		}

		[Test]
		public void ShouldReturnNullIfNotFound()
		{
			target.Find("not existing")
				.Should().Be.Null();
		}

		[SetUp]
		public void Setup_WillBeChangedWhenMovedAwayFromUnitOfWork()
		{
			using (var uow = UnitOfWorkFactory.Current.CreateAndOpenUnitOfWork())
			{
				correctUserName = "arna";
				var personInDatabase = PersonFactory.CreatePersonWithBasicPermissionInfo(correctUserName, "something");
				new PersonRepository(uow).Add(personInDatabase);
				uow.PersistAll();
				personId = personInDatabase.Id.Value;
			}
			_tenantUnitOfWorkManager = TenantUnitOfWorkManager.CreateInstanceForTest(ConnectionStringHelper.ConnectionStringUsedInTests);
			target = new ApplicationUserTenantQuery(_tenantUnitOfWorkManager);
		}
	}
}