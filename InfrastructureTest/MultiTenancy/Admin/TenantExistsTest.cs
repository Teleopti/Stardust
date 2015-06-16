﻿using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Infrastructure.MultiTenancy.Admin;
using Teleopti.Ccc.Infrastructure.MultiTenancy.Server;
using Teleopti.Ccc.Infrastructure.MultiTenancy.Server.NHibernate;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.TestData;

namespace Teleopti.Ccc.InfrastructureTest.MultiTenancy.Admin
{
	public class TenantExistsTest
	{
		private TenantUnitOfWorkManager tenantUnitOfWorkManager;

		[Test]
		public void ShouldExist()
		{
			var tenant = new Tenant(RandomName.Make());
			tenantUnitOfWorkManager.CurrentSession().Save(tenant);

			var target = new TenantExists(tenantUnitOfWorkManager);
			target.Check(tenant.Name)
				.Should().Be.True();
		}

		[Test]
		public void ShouldNotExist()
		{
			var target = new TenantExists(tenantUnitOfWorkManager);
			target.Check(RandomName.Make())
				.Should().Be.False();
		}

		[SetUp]
		public void Setup()
		{
			tenantUnitOfWorkManager = TenantUnitOfWorkManager.CreateInstanceForHostsWithOneUser(ConnectionStringHelper.ConnectionStringUsedInTests);
			tenantUnitOfWorkManager.Start();
		}

		[TearDown]
		public void RollbackTransaction()
		{
			tenantUnitOfWorkManager.Dispose();
		} 
	}
}