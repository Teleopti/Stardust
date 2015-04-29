using System;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Infrastructure.MultiTenancy.Server;
using Teleopti.Ccc.Infrastructure.MultiTenancy.Server.NHibernate;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.TestData;

namespace Teleopti.Ccc.InfrastructureTest.MultiTenancy.Server
{
	public class PersistPersonInfoTest
	{
		private Tenant tenant;
		private TenantUnitOfWorkManager _tenantUnitOfWorkManager;
		private IPersistPersonInfo target;

		[SetUp]
		public void InsertPreState()
		{
			_tenantUnitOfWorkManager = TenantUnitOfWorkManager.CreateInstanceForTest(ConnectionStringHelper.ConnectionStringUsedInTests);
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
			var newPassword = RandomName.Make();

			var session = _tenantUnitOfWorkManager.CurrentSession();

			var id = Guid.NewGuid();

			var personInfo = new PersonInfo(tenant, id);
			session.Save(personInfo);
			session.Flush();
			session.Clear();

			personInfo.SetApplicationLogonCredentials(new CheckPasswordStrengthFake(), RandomName.Make(), newPassword);
			target.Persist(personInfo);

			session.Flush();
			session.Clear();
			var loaded = session.Get<PersonInfo>(id);
			loaded.Password.Should().Be.EqualTo(EncryptPassword.ToDbFormat(newPassword));
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

			target.Persist(personInfo1);
			target.Persist(personInfo2);

			Assert.Throws<DuplicateApplicationLogonNameException>(() => _tenantUnitOfWorkManager.CurrentSession().Flush());
		}

		[Test]
		public void MultipleNullApplicationLogonShouldNotThrow()
		{
			var personInfo1 = new PersonInfo(tenant, Guid.NewGuid());
			personInfo1.SetApplicationLogonCredentials(new CheckPasswordStrengthFake(), null, RandomName.Make());
			var personInfo2 = new PersonInfo(tenant, Guid.NewGuid());
			personInfo2.SetApplicationLogonCredentials(new CheckPasswordStrengthFake(), null, RandomName.Make());

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
			target.Persist(personInfo2);

			Assert.Throws<DuplicateIdentityException>(() => _tenantUnitOfWorkManager.CurrentSession().Flush());
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
		public void ShouldThrowIfExplicitIdIsNotSet()
		{
			Assert.Throws<ArgumentException>(() =>
				target.Persist(new PersonInfo(tenant, Guid.Empty)));
		}
	}
}