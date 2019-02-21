using System;
using System.Linq;
using NHibernate;
using NHibernate.Exceptions;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Infrastructure.MultiTenancy.Admin;
using Teleopti.Ccc.Infrastructure.MultiTenancy.Server;
using Teleopti.Ccc.Infrastructure.MultiTenancy.Server.NHibernate;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.TestData;

namespace Teleopti.Ccc.InfrastructureTest.MultiTenancy.Admin
{
	public class DeleteTenantTest
	{
		private TenantUnitOfWorkManager tenantUnitOfWorkManager;

		[Test]
		public void ShouldDeleteTenant()
		{
			var tenantName = RandomName.Make();
			var session = tenantUnitOfWorkManager.CurrentSession();
			var tenant = createAndSaveTenant(session, tenantName);

			var target = new DeleteTenant(tenantUnitOfWorkManager);
			target.Delete(tenant);
			session.FlushAndClear();

			new LoadAllTenants(tenantUnitOfWorkManager).Tenants().Where(x => x.Name.Equals(tenantName)).Should().Be.Empty();
		}

		[Test]
		public void ShouldCascadeDeletePersonInfos()
		{
			var tenantName = RandomName.Make();
			var session = tenantUnitOfWorkManager.CurrentSession();
			var tenant = createAndSaveTenant(session, tenantName);
			var pInfo1 = new PersonInfo(tenant, Guid.NewGuid());
			var pInfo2 = new PersonInfo(tenant, Guid.NewGuid());
			session.Save(pInfo1);
			session.Save(pInfo2);
			session.FlushAndClear();

			var target = new DeleteTenant(tenantUnitOfWorkManager);
			target.Delete(tenant);
			session.FlushAndClear();

			session.CreateQuery("select pi from PersonInfo pi where tenant=:tenant")
				.SetEntity("tenant", tenant)
				.List<PersonInfo>()
				.Should().Be.Empty();
		}

		[Test]
		public void ShouldNotAllowPersonInfoOnDeletedTenant()
		{
			var tenantName = RandomName.Make();
			var session = tenantUnitOfWorkManager.CurrentSession();
			var tenant = createAndSaveTenant(session, tenantName);

			var target = new DeleteTenant(tenantUnitOfWorkManager);
			target.Delete(tenant);
			session.FlushAndClear();

			var pInfo1 = new PersonInfo(tenant, Guid.NewGuid());
			session.Save(pInfo1);
			Assert.Throws<GenericADOException>(() =>
				session.Flush());
		}

		private static Tenant createAndSaveTenant(ISession session, string tenantName)
		{
			var tenant = new Tenant(tenantName);
			session.Save(tenant);
			session.FlushAndClear();
			return tenant;
		}


		[SetUp]
		public void Setup()
		{
			tenantUnitOfWorkManager = TenantUnitOfWorkManager.Create(InfraTestConfigReader.ConnectionString);
			tenantUnitOfWorkManager.EnsureUnitOfWorkIsStarted();
		}

		[TearDown]
		public void RollbackTransaction()
		{
			tenantUnitOfWorkManager.Dispose();
		}
	}
}