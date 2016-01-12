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
	public class ApplicationUserQueryTest
	{
		private IApplicationUserQuery target;
		private TenantUnitOfWorkManager _tenantUnitOfWorkManager;
		private PersonInfo existingPerson;

		[Test]
		public void ShouldFindPersonId()
		{
			var result = target.Find(existingPerson.ApplicationLogonInfo.LogonName);
			result.Id.Should().Be.EqualTo(existingPerson.Id);
		}

		[Test]
		public void ShouldFindTenant()
		{
			var result = target.Find(existingPerson.ApplicationLogonInfo.LogonName);
			result.Tenant.Name.Should().Be.EqualTo(existingPerson.Tenant.Name);
		}

		[Test]
		public void ShouldReturnNullIfNotFound()
		{
			target.Find("not existing")
				.Should().Be.Null();
		}

		[SetUp]
		public void Setup()
		{
			_tenantUnitOfWorkManager = TenantUnitOfWorkManager.Create(ConnectionStringHelper.ConnectionStringUsedInTests);
			_tenantUnitOfWorkManager.EnsureUnitOfWorkIsStarted();
			var tenant = new Tenant(RandomName.Make());
			_tenantUnitOfWorkManager.CurrentSession().Save(tenant);
			existingPerson = new PersonInfo(tenant, Guid.NewGuid());
			existingPerson.SetApplicationLogonCredentials(new CheckPasswordStrengthFake(), RandomName.Make(), RandomName.Make());
			var personInfoPersister = new PersistPersonInfo(_tenantUnitOfWorkManager);
			personInfoPersister.Persist(existingPerson);
			_tenantUnitOfWorkManager.CurrentSession().Flush();
			target = new ApplicationUserQuery(_tenantUnitOfWorkManager);
		}

		[TearDown]
		public void Clean()
		{
			_tenantUnitOfWorkManager.CancelAndDisposeCurrent();
			_tenantUnitOfWorkManager.Dispose();
		}
	}
}