using System;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Infrastructure.MultiTenancy.Server;
using Teleopti.Ccc.Infrastructure.MultiTenancy.Server.NHibernate;
using Teleopti.Ccc.Infrastructure.MultiTenancy.Server.Queries;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.TestData;

namespace Teleopti.Ccc.InfrastructureTest.MultiTenancy.Server.Queries
{
	public class FindPersonInfoTest
	{
		private TenantUnitOfWorkManager tenantUnitOfWorkManager;
		private FindPersonInfo target;
		private Tenant tenant;

		[Test]
		public void ShouldFindUser()
		{
			var personId = Guid.NewGuid();
			var personInfo = new PersonInfo(tenant, personId);
			tenantUnitOfWorkManager.CurrentSession().Save(personInfo);

			target.GetById(personId).Should().Not.Be.Null();
		}

		[Test]
		public void ShouldReturnNullIfNotFound()
		{
			target.GetById(Guid.NewGuid()).Should().Be.Null();
		}


		[SetUp]
		public void InsertPreState()
		{
			tenantUnitOfWorkManager = TenantUnitOfWorkManager.Create(InfraTestConfigReader.ConnectionString);
			tenantUnitOfWorkManager.EnsureUnitOfWorkIsStarted();

			target = new FindPersonInfo(tenantUnitOfWorkManager);

			tenant = new Tenant(RandomName.Make());
			tenantUnitOfWorkManager.CurrentSession().Save(tenant);
		}

		[TearDown]
		public void Cleanup()
		{
			tenantUnitOfWorkManager.Dispose();
		}
	}
}