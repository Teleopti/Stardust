using NHibernate.Impl;
using NUnit.Framework;
using SharpTestsEx;
using System;
using System.Linq;
using Teleopti.Ccc.Domain.MultiTenancy;
using Teleopti.Ccc.Infrastructure.MultiTenancy.Server;
using Teleopti.Ccc.Infrastructure.MultiTenancy.Server.NHibernate;
using Teleopti.Ccc.Infrastructure.MultiTenancy.Server.Queries;
using Teleopti.Ccc.Infrastructure.Security;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.TestData;

namespace Teleopti.Ccc.InfrastructureTest.MultiTenancy.Server.Queries
{
	public class PersistPersonInfoTest
	{
		private Tenant tenant;
		private TenantUnitOfWorkManager _tenantUnitOfWorkManager;
		private IPersistPersonInfo target;
		private CurrentTenantUserFake currentTenant;
		private TenantAuditPersister tenantAuditPersister;

		[SetUp]
		public void InsertPreState()
		{
			_tenantUnitOfWorkManager = TenantUnitOfWorkManager.Create(InfraTestConfigReader.ConnectionString);
			_tenantUnitOfWorkManager.EnsureUnitOfWorkIsStarted();
			tenantAuditPersister = new TenantAuditPersister(_tenantUnitOfWorkManager);
			currentTenant = new CurrentTenantUserFake();
			target = new PersistPersonInfo(_tenantUnitOfWorkManager
				, new PersonInfoPersister(_tenantUnitOfWorkManager)
				, tenantAuditPersister
				, currentTenant);

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
			target.Persist(personInfo);

			session.Flush();
			session.Clear();
			session.Get<PersonInfo>(personInfo.Id).Should().Not.Be.Null();
		}

		[Test]
		public void ShouldAddAuditRecordsWhenPersistingPersonInfo()
		{
			var session = _tenantUnitOfWorkManager.CurrentSession();
			var currentUser = new PersonInfo(tenant, Guid.NewGuid());
			var personInfo = new PersonInfo(tenant, Guid.NewGuid());
			currentTenant.Set(currentUser);
			personInfo.SetApplicationLogonCredentials(new CheckPasswordStrengthFake(), "ashl10", "Pa$$w0rd", new OneWayEncryption());
			target.Persist(personInfo, PersistActionIntent.AppLogonChange);

			session.Flush();
			session.Get<PersonInfo>(personInfo.Id).Should().Not.Be.Null();

			var auditEntries = session.Query<TenantAudit>().ToList();
			auditEntries.Count.Should().Be.EqualTo(1);
			var theEntry = auditEntries.Single();
			theEntry.Action.Should().Be.EqualTo(PersistActionIntent.AppLogonChange.ToString());
			theEntry.Correlation.Should().Be.EqualTo(((SessionImpl)session).SessionId);
			theEntry.ActionPerformedOn.Should().Be.EqualTo(personInfo.Id);
			theEntry.ActionPerformedBy.Should().Be.EqualTo(currentUser.Id);
		}

		[Test]
		public void ShouldInsertNonExistingWithId()
		{
			var id = Guid.NewGuid();
			var session = _tenantUnitOfWorkManager.CurrentSession();

			var personInfo = new PersonInfo(tenant, id);
			target.Persist(personInfo);

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

			personInfo.SetApplicationLogonCredentials(new CheckPasswordStrengthFake(), newLogonName, RandomName.Make(), new OneWayEncryption());
			target.Persist(personInfo);

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
			personInfo1.SetApplicationLogonCredentials(new CheckPasswordStrengthFake(), logonName, RandomName.Make(), new OneWayEncryption());
			var personInfo2 = new PersonInfo(tenant, Guid.NewGuid());
			personInfo2.SetApplicationLogonCredentials(new CheckPasswordStrengthFake(), logonName, RandomName.Make(), new OneWayEncryption());

			target.Persist(personInfo1);
			_tenantUnitOfWorkManager.CurrentSession().Flush();
			Assert.Throws<DuplicateApplicationLogonNameException>(() => target.Persist(personInfo2));
		}

		[Test]
		public void MultipleNullApplicationLogonShouldNotThrow()
		{
			var personInfo1 = new PersonInfo(tenant, Guid.NewGuid());
			personInfo1.SetApplicationLogonCredentials(new CheckPasswordStrengthFake(), null, RandomName.Make(), new OneWayEncryption());
			var personInfo2 = new PersonInfo(tenant, Guid.NewGuid());
			personInfo2.SetApplicationLogonCredentials(new CheckPasswordStrengthFake(), null, RandomName.Make(), new OneWayEncryption());

			target.Persist(personInfo1);
			target.Persist(personInfo2);

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

			target.Persist(personInfo1);
			_tenantUnitOfWorkManager.CurrentSession().Flush();
			Assert.Throws<DuplicateIdentityException>(() => target.Persist(personInfo2));
		}

		[Test]
		public void MultipleNullIdentityShouldNotThrow()
		{
			var personInfo1 = new PersonInfo(tenant, Guid.NewGuid());
			personInfo1.SetIdentity(null);
			var personInfo2 = new PersonInfo(tenant, Guid.NewGuid());
			personInfo2.SetIdentity(null);

			target.Persist(personInfo1);
			target.Persist(personInfo2);

			Assert.DoesNotThrow(_tenantUnitOfWorkManager.CurrentSession().Flush);
		}

		[Test]
		public void ShouldNotOverwriteOldTenantPassword()
		{
			var session = _tenantUnitOfWorkManager.CurrentSession();
			var id = Guid.NewGuid();

			var personInfo = new PersonInfo(tenant, id);
			var oldTenantPassword = personInfo.TenantPassword; 
			
			target.Persist(personInfo);

			var personInfoNew = new PersonInfo(tenant, id);
			target.Persist(personInfoNew);
			
			var loaded = session.Get<PersonInfo>(id);
			var result = loaded.TenantPassword;
			result.Should().Be.EqualTo(oldTenantPassword);
			string.IsNullOrEmpty(result).Should().Be.False();
		}

		[Test]
		public void ShouldThrowIfExplicitIdIsNotSet()
		{
			Assert.Throws<ArgumentException>(() =>
				target.Persist(new PersonInfo(tenant, Guid.Empty)));
		}

		[Test]
		public void ShouldReusePassswordWhenUpdatingOldIfLogonNameSupplied()
		{
			var session = _tenantUnitOfWorkManager.CurrentSession();
			var id = Guid.NewGuid();

			var personInfo = new PersonInfo(tenant, id);
			personInfo.SetApplicationLogonCredentials(new CheckPasswordStrengthFake(), "logonName", RandomName.Make(), new OneWayEncryption());
			var oldPw = personInfo.ApplicationLogonInfo.LogonPassword;
			target.Persist(personInfo);

			var personInfoNew = new PersonInfo(tenant, id);
			personInfoNew.SetApplicationLogonCredentials(new CheckPasswordStrengthFake(), "newLogonName", string.Empty, new OneWayEncryption());
			target.Persist(personInfoNew);

			var loaded = session.Get<PersonInfo>(id);
			loaded.ApplicationLogonInfo.LogonName
				.Should().Be.EqualTo(personInfoNew.ApplicationLogonInfo.LogonName);
			loaded.ApplicationLogonInfo.LogonPassword
				.Should().Be.EqualTo(oldPw);
		}

		[Test]
		public void ShouldNotReuseLogonPasswordWhenNewSupplied()
		{
			var session = _tenantUnitOfWorkManager.CurrentSession();
			var id = Guid.NewGuid();

			var personInfo = new PersonInfo(tenant, id);
			personInfo.SetApplicationLogonCredentials(new CheckPasswordStrengthFake(), "whatever", RandomName.Make(), new OneWayEncryption());
			var pw = personInfo.ApplicationLogonInfo.LogonPassword;

			target.Persist(personInfo);

			var personInfoNew = new PersonInfo(tenant, id);
			personInfoNew.SetApplicationLogonCredentials(new CheckPasswordStrengthFake(), RandomName.Make(), "whatever", new OneWayEncryption());
			target.Persist(personInfoNew);
			

			var loaded = session.Get<PersonInfo>(id);
			loaded.ApplicationLogonInfo.LogonName
				.Should().Be.EqualTo(personInfoNew.ApplicationLogonInfo.LogonName);
			loaded.ApplicationLogonInfo.LogonPassword
				.Should().Not.Be.EqualTo(pw);
		}
	}
}