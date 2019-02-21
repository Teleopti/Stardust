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
	public class FindPersonInfoByCredentialsTest
	{
		private TenantUnitOfWorkManager tenantUnitOfWorkManager;
		private FindPersonInfoByCredentials target;
		private Tenant tenant;

		[Test]
		public void ShouldFindUser()
		{
			var personId = Guid.NewGuid();
			var personInfo = new PersonInfo(tenant, personId);
			tenantUnitOfWorkManager.CurrentSession().Save(personInfo);
			var tenantPassword = personInfo.TenantPassword;

			target.Find(personId, tenantPassword).Should().Not.Be.Null();
		}

		[Test]
		public void ShouldReturnNullIfIncorrectPersonId()
		{
			var personId = Guid.NewGuid();
			var personInfo = new PersonInfo(tenant, personId);
			tenantUnitOfWorkManager.CurrentSession().Save(personInfo);
			var tenantPassword = personInfo.TenantPassword;

			target.Find(Guid.NewGuid(), tenantPassword).Should().Be.Null();
		}

		[Test]
		public void ShouldReturnNullIfIncorrectTenantPassword()
		{
			var personId = Guid.NewGuid();
			var personInfo = new PersonInfo(tenant, personId);
			tenantUnitOfWorkManager.CurrentSession().Save(personInfo);

			target.Find(Guid.NewGuid(), RandomName.Make()).Should().Be.Null();
		}


		[SetUp]
		public void InsertPreState()
		{
			tenantUnitOfWorkManager = TenantUnitOfWorkManager.Create(InfraTestConfigReader.ConnectionString);
			tenantUnitOfWorkManager.EnsureUnitOfWorkIsStarted();

			target = new FindPersonInfoByCredentials(tenantUnitOfWorkManager);

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