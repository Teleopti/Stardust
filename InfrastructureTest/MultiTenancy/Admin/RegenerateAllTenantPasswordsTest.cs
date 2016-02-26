using System;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Infrastructure.MultiTenancy.Admin;
using Teleopti.Ccc.Infrastructure.MultiTenancy.Server;
using Teleopti.Ccc.Infrastructure.MultiTenancy.Server.NHibernate;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.TestData;

namespace Teleopti.Ccc.InfrastructureTest.MultiTenancy.Admin
{
	public class RegenerateAllTenantPasswordsTest
	{
		private TenantUnitOfWorkManager _tenantUnitOfWorkManager;

		[Test]
		public void HappyPath()
		{
			var session = _tenantUnitOfWorkManager.CurrentSession();
			var tenant = new Tenant(RandomName.Make());
			var personInfo = new PersonInfo(tenant, Guid.NewGuid());
			session.Save(tenant);
			session.Save(personInfo);
			var pwBefore = personInfo.TenantPassword;

			var target = new RegenerateAllTenantPasswords(_tenantUnitOfWorkManager);
			target.Modify();

			personInfo.TenantPassword.Should().Not.Be.EqualTo(pwBefore);
		}

		[Test]
		public void ShouldDoForAllTenants()
		{
			var session = _tenantUnitOfWorkManager.CurrentSession();
			var tenant1 = new Tenant(RandomName.Make());
			var tenant2 = new Tenant(RandomName.Make());
			var personInfo1 = new PersonInfo(tenant1, Guid.NewGuid());
			var personInfo2 = new PersonInfo(tenant2, Guid.NewGuid());
			session.Save(tenant1);
			session.Save(tenant2);
			session.Save(personInfo1);
			session.Save(personInfo2);
			var pwBefore1 = personInfo1.TenantPassword;
			var pwBefore2 = personInfo2.TenantPassword;

			var target = new RegenerateAllTenantPasswords(_tenantUnitOfWorkManager);
			target.Modify();

			personInfo1.TenantPassword.Should().Not.Be.EqualTo(pwBefore1);
			personInfo2.TenantPassword.Should().Not.Be.EqualTo(pwBefore2);
		}


		[SetUp]
		public void InsertPreState()
		{
			_tenantUnitOfWorkManager = TenantUnitOfWorkManager.Create(InfraTestConfigReader.ConnectionString);
			_tenantUnitOfWorkManager.EnsureUnitOfWorkIsStarted();
		}

		[TearDown]
		public void RollbackTransaction()
		{
			_tenantUnitOfWorkManager.Dispose();
		}
	}
}