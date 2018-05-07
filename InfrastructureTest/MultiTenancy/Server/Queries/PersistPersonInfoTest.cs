using NUnit.Framework;
using SharpTestsEx;
using System;
using System.Linq;
using Teleopti.Ccc.Domain.MultiTenancy;
using Teleopti.Ccc.Infrastructure.MultiTenancy.Server;
using Teleopti.Ccc.Infrastructure.MultiTenancy.Server.Aspects;
using Teleopti.Ccc.Infrastructure.MultiTenancy.Server.NHibernate;
using Teleopti.Ccc.Infrastructure.MultiTenancy.Server.Queries;
using Teleopti.Ccc.Infrastructure.Security;
using Teleopti.Ccc.IocCommon;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.IoC;
using Teleopti.Ccc.TestCommon.TestData;
using Teleopti.Ccc.TestCommon.Web;

namespace Teleopti.Ccc.InfrastructureTest.MultiTenancy.Server.Queries
{
	[TestFixture]
	[InfrastructureTest]
	public class PersistPersonInfoTest : IExtendSystem
	{
		private Tenant tenant;
		private TenantUnitOfWorkManager _tenantUnitOfWorkManager;

		public IPersistPersonInfo target;

		public void Extend(IExtend extend, IIocConfiguration configuration)
		{
			extend.AddService<PersistPersonInfo>(true);
			extend.AddService<TenantAuditAttribute>();
			extend.AddService<FakeCurrentHttpContext>();
		}

		[SetUp]
		public void InsertPreState()
		{
			_tenantUnitOfWorkManager = TenantUnitOfWorkManager.Create(InfraTestConfigReader.ConnectionString);
			_tenantUnitOfWorkManager.EnsureUnitOfWorkIsStarted();
			tenant = new Tenant(RandomName.Make());
			_tenantUnitOfWorkManager.CurrentSession().Save(tenant);
		}

		[TearDown]
		public void Cleanup()
		{
			_tenantUnitOfWorkManager.CurrentSession().CreateSQLQuery("TRUNCATE TABLE tenant.audit").ExecuteUpdate();
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

		[Test]
		public void ApplicationLogonNameUpdateShouldGenerateAuditTrailRecord()
		{
			var session = _tenantUnitOfWorkManager.CurrentSession();

			var personInfo = new PersonInfo(tenant, Guid.NewGuid());
			personInfo.SetApplicationLogonCredentials(new CheckPasswordStrengthFake(), RandomName.Make(), "password1", new OneWayEncryption());
			target.PersistApplicationLogonName(personInfo);

			var p1 = session.Get<PersonInfo>(personInfo.Id);
			var auditRecords = session.Query<TenantAudit>().ToList();

			auditRecords.Count.Should().Be.EqualTo(1);
			var auditRecord = auditRecords.Single();
			auditRecord.ActionPerformedOn.Should().Be.EqualTo(p1.Id);
			auditRecord.Action.Should().Be.EqualTo(PersistActionIntent.AppLogonChange.ToString());
			auditRecord.Correlation.Should().Be.EqualTo(_tenantUnitOfWorkManager.CurrentSessionId());
		}

		[Test]
		public void IdentityUpdateShouldGenerateAuditTrailRecord()
		{
			var session = _tenantUnitOfWorkManager.CurrentSession();

			var personInfo = new PersonInfo(tenant, Guid.NewGuid());
			personInfo.SetApplicationLogonCredentials(new CheckPasswordStrengthFake(), RandomName.Make(), "password1", new OneWayEncryption());
			personInfo.SetIdentity(RandomName.Make());
			target.PersistIdentity(personInfo);

			var p1 = session.Get<PersonInfo>(personInfo.Id);
			var auditRecords = session.Query<TenantAudit>().ToList();

			auditRecords.Count.Should().Be.EqualTo(1);
			var auditRecord = auditRecords.Single();
			auditRecord.ActionPerformedOn.Should().Be.EqualTo(p1.Id);
			auditRecord.Action.Should().Be.EqualTo(PersistActionIntent.IdentityChange.ToString());
			auditRecord.Correlation.Should().Be.EqualTo(_tenantUnitOfWorkManager.CurrentSessionId());


		}

		[Test]
		public void PersistShouldGenerateAuditTrailRecord()
		{
			var session = _tenantUnitOfWorkManager.CurrentSession();

			var personInfo = new PersonInfo(tenant, Guid.NewGuid());
			personInfo.SetApplicationLogonCredentials(new CheckPasswordStrengthFake(), RandomName.Make(), "password1", new OneWayEncryption());
			target.Persist(personInfo);

			var p1 = session.Get<PersonInfo>(personInfo.Id);
			var auditRecords = session.Query<TenantAudit>().ToList();

			auditRecords.Count.Should().Be.EqualTo(1);
			var auditRecord = auditRecords.Single();
			auditRecord.ActionPerformedOn.Should().Be.EqualTo(p1.Id);
			auditRecord.Action.Should().Be.EqualTo(PersistActionIntent.GenericPersistApiCall.ToString());
			auditRecord.Correlation.Should().Be.EqualTo(_tenantUnitOfWorkManager.CurrentSessionId());
		}

	}
}