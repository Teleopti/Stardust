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
using Teleopti.Ccc.IocCommon.Configuration;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.IoC;
using Teleopti.Ccc.TestCommon.TestData;
using Teleopti.Ccc.TestCommon.Web;

namespace Teleopti.Ccc.InfrastructureTest.MultiTenancy.Server.Queries
{
	[TestFixture]
	[InfrastructureTest]
	public class PersistPersonInfoTest : BasePersistPersonInfoTest, IExtendSystem
	{
		public void Extend(IExtend extend, IocConfiguration configuration)
		{
			extend.AddService<PersistPersonInfo>(true);
			extend.AddService<TenantAuditAttribute>();
			extend.AddService<FakeCurrentHttpContext>();
		}
	}

	[TestFixture]
	[InfrastructureTest]
	public class PersistPersonInfoWithAuditTrailCompatibilityTest : BasePersistPersonInfoTest, IExtendSystem
	{
		public void Extend(IExtend extend, IocConfiguration configuration)
		{
			extend.AddService<PersistPersonInfo>(true);
			extend.AddService<TenantAuditAttribute>();
			extend.AddService<FakeCurrentHttpContext>();
			extend.AddModule(new AuditTrailModule(configuration));
		}
	}

	public abstract class BasePersistPersonInfoTest
	{
		private Tenant tenant;
		private TenantUnitOfWorkManager _tenantUnitOfWorkManager;

		public IPersistPersonInfo Target;

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
		public void ShouldInsertNonExistingPersonInfoWithId()
		{
			var id = Guid.NewGuid();
			var session = _tenantUnitOfWorkManager.CurrentSession();

			var personInfo = new PersonInfo(tenant, id);
			personInfo.SetIdentity("DOMAIN/User1");
			Target.Persist(new GenericPersistApiCallActionObj(){PersonInfo = personInfo});

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
			Target.Persist(new GenericPersistApiCallActionObj() { PersonInfo = personInfo });

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

			Target.Persist(new GenericPersistApiCallActionObj() { PersonInfo = personInfo1 });
			_tenantUnitOfWorkManager.CurrentSession().Flush();
			Assert.Throws<DuplicateApplicationLogonNameException>(() => Target.Persist(new GenericPersistApiCallActionObj() { PersonInfo = personInfo2 }));
		}

		[Test]
		public void MultipleNullApplicationLogonShouldNotThrow()
		{
			var personInfo1 = new PersonInfo(tenant, Guid.NewGuid());
			personInfo1.SetApplicationLogonCredentials(new CheckPasswordStrengthFake(), null, RandomName.Make(), new OneWayEncryption());
			var personInfo2 = new PersonInfo(tenant, Guid.NewGuid());
			personInfo2.SetApplicationLogonCredentials(new CheckPasswordStrengthFake(), null, RandomName.Make(), new OneWayEncryption());

			Target.Persist(new GenericPersistApiCallActionObj() { PersonInfo = personInfo1 });
			Target.Persist(new GenericPersistApiCallActionObj() { PersonInfo = personInfo2 });

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

			Target.Persist(new GenericPersistApiCallActionObj() { PersonInfo = personInfo1 });
			_tenantUnitOfWorkManager.CurrentSession().Flush();
			Assert.Throws<DuplicateIdentityException>(() => Target.Persist(new GenericPersistApiCallActionObj() { PersonInfo = personInfo2 }));
		}

		[Test]
		public void MultipleNullIdentityShouldNotThrowOrSave()
		{
			var personInfo1 = new PersonInfo(tenant, Guid.NewGuid());
			personInfo1.SetIdentity(null);
			var personInfo2 = new PersonInfo(tenant, Guid.NewGuid());
			personInfo2.SetIdentity(null);

			Target.Persist(new GenericPersistApiCallActionObj() { PersonInfo = personInfo1 });
			Target.Persist(new GenericPersistApiCallActionObj() { PersonInfo = personInfo2 });

			var result = _tenantUnitOfWorkManager.CurrentSession().Query<PersonInfo>().ToList();
			result.Count.Should().Be(0);

			Assert.DoesNotThrow(_tenantUnitOfWorkManager.CurrentSession().Flush);
		}

		[Test]
		public void ShouldNotPersistEmptyIdentityPersonInfoRecords()
		{
			var session = _tenantUnitOfWorkManager.CurrentSession();

			var personInfo1 = new PersonInfo(tenant, Guid.NewGuid());
			personInfo1.ApplicationLogonInfo.SetLogonName(string.Empty);
			Target.PersistIdentity(new IdentityChangeActionObj() { PersonInfo = personInfo1 });
			Assert.DoesNotThrow(session.Flush);
			
			var personInfo2 = new PersonInfo(tenant, Guid.NewGuid());
			personInfo2.ApplicationLogonInfo.SetLogonName(string.Empty);
			Target.PersistIdentity(new IdentityChangeActionObj() { PersonInfo = personInfo2 });
			Assert.DoesNotThrow(session.Flush);

			var result = session.Query<PersonInfo>().ToList();
			result.Count.Should().Be(0);
		}

		[Test]
		public void ShouldNotPersistEmptyApplicationLogonPersonInfoRecords()
		{
			var session = _tenantUnitOfWorkManager.CurrentSession();

			var personInfo1 = new PersonInfo(tenant, Guid.NewGuid());
			Target.PersistApplicationLogonName(new AppLogonChangeActionObj() { PersonInfo = personInfo1 });
			Assert.DoesNotThrow(session.Flush);

			var personInfo2 = new PersonInfo(tenant, Guid.NewGuid());
			Target.PersistApplicationLogonName(new AppLogonChangeActionObj() { PersonInfo = personInfo2 });
			Assert.DoesNotThrow(session.Flush);

			var result = session.Query<PersonInfo>().ToList();
			result.Count.Should().Be(0);
		}

		[Test]
		public void ShouldNotOverwriteOldTenantPassword()
		{
			var session = _tenantUnitOfWorkManager.CurrentSession();
			var id = Guid.NewGuid();

			var personInfo = new PersonInfo(tenant, id);
			personInfo.SetIdentity("DOMAIN/User1");
			var oldTenantPassword = personInfo.TenantPassword; 
			
			Target.Persist(new GenericPersistApiCallActionObj() { PersonInfo = personInfo });

			var personInfoNew = new PersonInfo(tenant, id);
			personInfoNew.SetIdentity("DOMAIN/User1");
			Target.Persist(new GenericPersistApiCallActionObj() { PersonInfo = personInfoNew });
			
			var loaded = session.Get<PersonInfo>(id);
			var result = loaded.TenantPassword;
			result.Should().Be.EqualTo(oldTenantPassword);
			string.IsNullOrEmpty(result).Should().Be.False();
		}

		[Test]
		public void ShouldThrowIfExplicitIdIsNotSet()
		{
			Assert.Throws<ArgumentException>(() =>
				Target.Persist(new GenericPersistApiCallActionObj() { PersonInfo = new PersonInfo(tenant, Guid.Empty)}));
		}

		[Test]
		public void ShouldReusePassswordWhenUpdatingOldIfLogonNameSupplied()
		{
			var session = _tenantUnitOfWorkManager.CurrentSession();
			var id = Guid.NewGuid();

			var personInfo = new PersonInfo(tenant, id);
			personInfo.SetApplicationLogonCredentials(new CheckPasswordStrengthFake(), "logonName", RandomName.Make(), new OneWayEncryption());
			var oldPw = personInfo.ApplicationLogonInfo.LogonPassword;
			Target.Persist(new GenericPersistApiCallActionObj() { PersonInfo = personInfo });

			var personInfoNew = new PersonInfo(tenant, id);
			personInfoNew.SetApplicationLogonCredentials(new CheckPasswordStrengthFake(), "newLogonName", string.Empty, new OneWayEncryption());
			Target.Persist(new GenericPersistApiCallActionObj() { PersonInfo = personInfoNew });

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

			Target.Persist(new GenericPersistApiCallActionObj() { PersonInfo = personInfo });

			var personInfoNew = new PersonInfo(tenant, id);
			personInfoNew.SetApplicationLogonCredentials(new CheckPasswordStrengthFake(), RandomName.Make(), "whatever", new OneWayEncryption());
			Target.Persist(new GenericPersistApiCallActionObj() { PersonInfo = personInfoNew });
			

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
			Target.PersistApplicationLogonName(new AppLogonChangeActionObj() { PersonInfo = personInfo });

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
			Target.PersistIdentity(new IdentityChangeActionObj {PersonInfo = personInfo});

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
			Target.Persist(new GenericPersistApiCallActionObj() { PersonInfo = personInfo });

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