using System;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Infrastructure.MultiTenancy.Server;
using Teleopti.Ccc.Infrastructure.MultiTenancy.Server.NHibernate;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Ccc.Infrastructure.UnitOfWork;
using Teleopti.Ccc.InfrastructureTest.Helper;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.TestCommon.TestData;

namespace Teleopti.Ccc.InfrastructureTest.MultiTenancy.Server
{
	public class ApplicationUserQueryTest
	{
		private Guid personId;
		private string correctUserName;
		private IApplicationUserQuery target;
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


		[Test]
		public void ShouldFindPassword()
		{
			var result = target.Find(correctUserName);
			result.Password.Should().Not.Be.Null();
		}


		[SetUp]
		public void Setup()
		{
			correctUserName = RandomName.Make();
			_tenantUnitOfWorkManager = TenantUnitOfWorkManager.CreateInstanceForTest(ConnectionStringHelper.ConnectionStringUsedInTests);
			var tenant = new FindTenantByNameQuery(_tenantUnitOfWorkManager).Find(Tenant.DefaultName);
			personId = Guid.NewGuid();
			var pInfo = new PersonInfo(tenant, personId);
			pInfo.SetApplicationLogonCredentials(new CheckPasswordStrengthFake(), correctUserName, RandomName.Make());
			var personInfoPersister = new PersistPersonInfo(_tenantUnitOfWorkManager);
			personInfoPersister.Persist(pInfo);
			_tenantUnitOfWorkManager.CurrentSession().Flush();
			target = new ApplicationUserQuery(_tenantUnitOfWorkManager);
		}

		[TearDown]
		public void Clean()
		{
			_tenantUnitOfWorkManager.Dispose();
		}
	}

	public class ApplicationUserQueryTest_OldSchema_RemoveMeWhenToggleIsGone : DatabaseTestWithoutTransaction
	{
		private Guid personId;
		private string correctUserName;
		private IApplicationUserQuery target;
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

		//TODO: tenant - this could be removed when old schema is removed
		[Test]
		public void ShouldWorkIfUserCredentialsAlsoInNewSchema()
		{
			var persister = new PersistPersonInfo(_tenantUnitOfWorkManager);
			var person = Session.Get<Person>(personId);
			var tenant = new Tenant(RandomName.Make());
			_tenantUnitOfWorkManager.CurrentSession().Save("Tenant_NewSchema", tenant);
			var personInfo = new PersonInfo(tenant, person.Id.Value);
			personInfo.SetApplicationLogonCredentials(new CheckPasswordStrengthFake(), person.ApplicationAuthenticationInfo.ApplicationLogOnName, person.ApplicationAuthenticationInfo.Password);
			persister.Persist(personInfo);
			_tenantUnitOfWorkManager.CurrentSession().Flush();
			target.Find(person.ApplicationAuthenticationInfo.ApplicationLogOnName).Id
				.Should().Be.EqualTo(personId);
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
			target = new ApplicationUserQueryOldSchema(_tenantUnitOfWorkManager);
		}
	}
}