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
	public class PersistPersonInfoTest
	{
		private Tenant tenant;
		private TenantUnitOfWorkManager _tenantUnitOfWorkManager;
		private IPersistPersonInfo target;

		[SetUp]
		public void InsertPreState()
		{
			_tenantUnitOfWorkManager = TenantUnitOfWorkManager.CreateInstanceForHostsWithOneUser(ConnectionStringHelper.ConnectionStringUsedInTests);
			_tenantUnitOfWorkManager.Start();

			target = new PersistPersonInfo(_tenantUnitOfWorkManager);

			tenant = new Tenant(RandomName.Make());
			_tenantUnitOfWorkManager.CurrentSession().Save(tenant);
		}

		[TearDown]
		public void Cleanup()
		{
			_tenantUnitOfWorkManager.Dispose();
		}

		[Test]
		public void ShouldInsertPersonInfo()
		{
			var session = _tenantUnitOfWorkManager.CurrentSession(); 
			
			var personInfo = new PersonInfo(tenant, Guid.NewGuid());
			target.Persist(personInfo, "",false);

			session.Flush();
			session.Clear();
			session.Get<PersonInfo>(personInfo.Id).Should().Not.Be.Null();
		}

		[Test]
		public void ShouldInsertNonExistingWithId()
		{
			var id = Guid.NewGuid();
			var session = _tenantUnitOfWorkManager.CurrentSession();

			var personInfo = new PersonInfo(tenant, id);
			target.Persist(personInfo,"", false);

			session.Flush();
			session.Clear();
			session.Get<PersonInfo>(personInfo.Id).Id.Should().Be.EqualTo(id);
		}

		[Test]
		public void ShouldUpdatePersonInfo()
		{
			var newLogonName = RandomName.Make();

			var session = _tenantUnitOfWorkManager.CurrentSession();

			var id = Guid.NewGuid();

			var personInfo = new PersonInfo(tenant, id);
			session.Save(personInfo);
			session.Flush();
			session.Clear();

			personInfo.SetApplicationLogonCredentials(new CheckPasswordStrengthFake(), newLogonName, RandomName.Make());
			target.Persist(personInfo,"", false);

			session.Flush();
			session.Clear();
			var loaded = session.Get<PersonInfo>(id);
			loaded.ApplicationLogonInfo.LogonName.Should().Be.EqualTo(newLogonName);
			loaded.Id.Should().Be.EqualTo(id);
		}

		[Test]
		public void SameApplicationLogonShouldThrow()
		{
			var logonName = RandomName.Make();

			var personInfo1 = new PersonInfo(tenant, Guid.NewGuid());
			personInfo1.SetApplicationLogonCredentials(new CheckPasswordStrengthFake(), logonName, RandomName.Make());
			var personInfo2 = new PersonInfo(tenant, Guid.NewGuid());
			personInfo2.SetApplicationLogonCredentials(new CheckPasswordStrengthFake(), logonName, RandomName.Make());

			target.Persist(personInfo1,"", false);
			target.Persist(personInfo2,"", false);

			Assert.Throws<DuplicateApplicationLogonNameException>(() => _tenantUnitOfWorkManager.CurrentSession().Flush());
		}

		[Test]
		public void MultipleNullApplicationLogonShouldNotThrow()
		{
			var personInfo1 = new PersonInfo(tenant, Guid.NewGuid());
			personInfo1.SetApplicationLogonCredentials(new CheckPasswordStrengthFake(), null, RandomName.Make());
			var personInfo2 = new PersonInfo(tenant, Guid.NewGuid());
			personInfo2.SetApplicationLogonCredentials(new CheckPasswordStrengthFake(), null, RandomName.Make());

			target.Persist(personInfo1,"", false);
			target.Persist(personInfo2,"",false);

			Assert.DoesNotThrow(_tenantUnitOfWorkManager.CurrentSession().Flush);
		}

		[Test]
		public void SameIdentityShouldThrow()
		{
			var identity = RandomName.Make();

			var personInfo1 = new PersonInfo(tenant, Guid.NewGuid());
			personInfo1.SetIdentity(identity);
			var personInfo2 = new PersonInfo(tenant, Guid.NewGuid());
			personInfo2.SetIdentity(identity);

			target.Persist(personInfo1,"", false);
			target.Persist(personInfo2,"", false);

			Assert.Throws<DuplicateIdentityException>(() => _tenantUnitOfWorkManager.CurrentSession().Flush());
		}

		[Test]
		public void MultipleNullIdentityShouldNotThrow()
		{
			var personInfo1 = new PersonInfo(tenant, Guid.NewGuid());
			personInfo1.SetIdentity(null);
			var personInfo2 = new PersonInfo(tenant, Guid.NewGuid());
			personInfo2.SetIdentity(null);

			target.Persist(personInfo1,"", false);
			target.Persist(personInfo2,"", false);

			Assert.DoesNotThrow(_tenantUnitOfWorkManager.CurrentSession().Flush);
		}

		[Test]
		public void ShouldNotOverwriteOldTenantPassword()
		{
			var session = _tenantUnitOfWorkManager.CurrentSession();
			var id = Guid.NewGuid();

			var personInfo = new PersonInfo(tenant, id);
			var oldTenantPassword = personInfo.TenantPassword; 
			
			target.Persist(personInfo,"", false);

			var personInfoNew = new PersonInfo(tenant, id);
			target.Persist(personInfoNew,"", false);
			
			var loaded = session.Get<PersonInfo>(id);
			var result = loaded.TenantPassword;
			result.Should().Be.EqualTo(oldTenantPassword);
			string.IsNullOrEmpty(result).Should().Be.False();
		}

		[Test]
		public void ShouldThrowIfExplicitIdIsNotSet()
		{
			Assert.Throws<ArgumentException>(() =>
				target.Persist(new PersonInfo(tenant, Guid.Empty),"", false));
		}

		[Test]
		public void ShouldReuseLogonWhenUpdatingOldIfLogonNameSupplied()
		{
			var session = _tenantUnitOfWorkManager.CurrentSession();
			var id = Guid.NewGuid();

			var personInfo = new PersonInfo(tenant, id);
			personInfo.SetApplicationLogonCredentials(new CheckPasswordStrengthFake(), "logonName", "Password");
			var pw = personInfo.ApplicationLogonInfo.LogonPassword;

			target.Persist(personInfo, "", false);

			var personInfoNew = new PersonInfo(tenant, id);
			target.Persist(personInfoNew, "newlogonName", true);

			var loaded = session.Get<PersonInfo>(id);
			var result = loaded.ApplicationLogonInfo.LogonPassword;
			result.Should().Not.Be.Empty();
			result = loaded.ApplicationLogonInfo.LogonName;
			result.Should().Be.EqualTo("newlogonName");
			result = loaded.ApplicationLogonInfo.LogonPassword;
			result.Should().Be.EqualTo(pw);
		}

		[Test]
		public void ShouldNotReuseLogonPasswordWhenNewSupplied()
		{
			var session = _tenantUnitOfWorkManager.CurrentSession();
			var id = Guid.NewGuid();

			var personInfo = new PersonInfo(tenant, id);
			personInfo.SetApplicationLogonCredentials(new CheckPasswordStrengthFake(), "logonName", "Password");
			var pw = personInfo.ApplicationLogonInfo.LogonPassword;

			target.Persist(personInfo, "", false);

			var personInfoNew = new PersonInfo(tenant, id);
			personInfoNew.SetApplicationLogonCredentials(new CheckPasswordStrengthFake(), "logonName2", "AnotherPassword");
			target.Persist(personInfoNew, "shouldnotbeused", false);
			

			var loaded = session.Get<PersonInfo>(id);
			var result = loaded.ApplicationLogonInfo.LogonPassword;
			result.Should().Not.Be.Empty();
			result = loaded.ApplicationLogonInfo.LogonName;
			result.Should().Be.EqualTo("logonName2");
			result = loaded.ApplicationLogonInfo.LogonPassword;
			result.Should().Not.Be.EqualTo(pw);
		}
	}
}