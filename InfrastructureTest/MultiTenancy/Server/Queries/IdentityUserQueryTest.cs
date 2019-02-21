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
	public class IdentityUserQueryTest
	{
		private IIdentityUserQuery target;
		private TenantUnitOfWorkManager _tenantUnitOfWorkManager;
		private PersonInfo existingPerson;

		[Test]
		public void ShouldFindPersonId()
		{
			var result = target.FindUserData(existingPerson.Identity);
			result.Id.Should().Be.EqualTo(existingPerson.Id);
		}

		[Test]
		public void ShouldFindTenant()
		{
			var result = target.FindUserData(existingPerson.Identity);
			result.Tenant.Name.Should().Be.EqualTo(existingPerson.Tenant.Name);
		}

		[Test]
		public void NonExistingUserShouldFail()
		{
			var result = target.FindUserData("incorrectUserName");
			result.Should().Be.Null();
		}

        [Test]
        public void ShouldReturnNullIfNotActiveTenant()
        {
            var before = target.FindUserData(existingPerson.Identity);
            before.Tenant.Active = false;
            _tenantUnitOfWorkManager.CurrentSession().Save(before);
            _tenantUnitOfWorkManager.CurrentSession().Flush();
            target.FindUserData(existingPerson.Identity)
                    .Should().Be.Null();
        }

        [SetUp]
		public void Setup_WillBeChangedWhenMovedAwayFromUnitOfWork()
		{
			_tenantUnitOfWorkManager = TenantUnitOfWorkManager.Create(InfraTestConfigReader.ConnectionString);
			_tenantUnitOfWorkManager.EnsureUnitOfWorkIsStarted();

			var tenant = new Tenant(RandomName.Make());
			_tenantUnitOfWorkManager.CurrentSession().Save(tenant);
			existingPerson = new PersonInfo(tenant, Guid.NewGuid());
			existingPerson.SetIdentity(RandomName.Make());

			var personInfoPersister = new PersistPersonInfo(_tenantUnitOfWorkManager, new PersonInfoPersister(_tenantUnitOfWorkManager));

			personInfoPersister.Persist(new GenericPersistApiCallActionObj() { PersonInfo = existingPerson});
			target = new IdentityUserQuery(_tenantUnitOfWorkManager);
		}

		[TearDown]
		public void Teardown_WillBeChangedWhenMovedAwayFromUnitOfWork()
		{
			_tenantUnitOfWorkManager.Dispose();
		}
	}
}