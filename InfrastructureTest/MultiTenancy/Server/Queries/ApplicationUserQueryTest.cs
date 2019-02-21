using System;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Infrastructure.MultiTenancy.Server;
using Teleopti.Ccc.Infrastructure.MultiTenancy.Server.NHibernate;
using Teleopti.Ccc.Infrastructure.MultiTenancy.Server.Queries;
using Teleopti.Ccc.Infrastructure.Security;
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

        [Test]
        public void ShouldReturnNullIfNotActiveTenant()
        {
            var before = target.Find(existingPerson.ApplicationLogonInfo.LogonName);
            before.Tenant.Active = false;
            _tenantUnitOfWorkManager.CurrentSession().Save(before);
            _tenantUnitOfWorkManager.CurrentSession().Flush();
            target.Find(existingPerson.ApplicationLogonInfo.LogonName)
                    .Should().Be.Null();
        }

		[SetUp]
		public void Setup()
		{
			_tenantUnitOfWorkManager = TenantUnitOfWorkManager.Create(InfraTestConfigReader.ApplicationConnectionString());
			_tenantUnitOfWorkManager.EnsureUnitOfWorkIsStarted();
			var tenant = new Tenant(RandomName.Make());
			_tenantUnitOfWorkManager.CurrentSession().Save(tenant);
			existingPerson = new PersonInfo(tenant, Guid.NewGuid());
			existingPerson.SetApplicationLogonCredentials(new CheckPasswordStrengthFake(), RandomName.Make(), RandomName.Make(), new OneWayEncryption());

			var personInfoPersister = new PersistPersonInfo(_tenantUnitOfWorkManager, new PersonInfoPersister(_tenantUnitOfWorkManager));
			personInfoPersister.Persist(new GenericPersistApiCallActionObj() { PersonInfo = existingPerson});
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