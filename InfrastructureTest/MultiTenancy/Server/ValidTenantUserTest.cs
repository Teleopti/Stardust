using System;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Infrastructure.MultiTenancy.Server;
using Teleopti.Ccc.Infrastructure.MultiTenancy.Server.NHibernate;
using Teleopti.Ccc.TestCommon;

namespace Teleopti.Ccc.InfrastructureTest.MultiTenancy.Server
{
	public class ValidTenantUserTest
	{
		private TenantUnitOfWorkManager tenantUnitOfWorkManager;
		private PersonInfo validPerson;
		private ValidTenantUser target;

		[Test]
		public void HappyPath()
		{
			target.IsValidForTenant(validPerson.Id, validPerson.TenantPassword)
				.Name.Should().Be.EqualTo(validPerson.Tenant.Name);
		}

		[Test]
		public void ShouldFailIfRegeneratePassword()
		{
			var oldPassword = validPerson.TenantPassword;
			validPerson.RegenerateTenantPassword();
			Assert.Throws<TenantPermissionException>(() =>
				target.IsValidForTenant(validPerson.Id, oldPassword)
			);
		}

		[Test]
		public void ShouldFailIfIncorrectPersonId()
		{
			Assert.Throws<TenantPermissionException>(() =>
				target.IsValidForTenant(Guid.NewGuid(), validPerson.TenantPassword)
			);
		}

		[Test]
		public void ShouldFailIfIncorrectPassword()
		{
			Assert.Throws<TenantPermissionException>(() =>
				target.IsValidForTenant(validPerson.Id, Guid.NewGuid().ToString())
			);
		}



		[SetUp]
		public void Setup()
		{
			tenantUnitOfWorkManager = TenantUnitOfWorkManager.CreateInstanceForHostsWithOneUser(ConnectionStringHelper.ConnectionStringUsedInTests);
			var tenant = new FindTenantByNameQuery(tenantUnitOfWorkManager).Find(Tenant.DefaultName);
			validPerson = new PersonInfo(tenant, Guid.NewGuid());
			var personInfoPersister = new PersistPersonInfo(tenantUnitOfWorkManager);
			personInfoPersister.Persist(validPerson);
			target = new ValidTenantUser(tenantUnitOfWorkManager);
		}

		[TearDown]
		public void Clean()
		{
			tenantUnitOfWorkManager.Dispose();
		}
	}
}