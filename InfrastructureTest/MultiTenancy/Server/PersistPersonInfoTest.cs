using System;
using NHibernate.Exceptions;
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
		private TenantUnitOfWorkManager tenantUnitOfWorkManager;
		private IPersistPersonInfo target;

		[SetUp]
		public void InsertPreState()
		{
			tenantUnitOfWorkManager = TenantUnitOfWorkManager.CreateInstanceForTest(ConnectionStringHelper.ConnectionStringUsedInTests);
			target = new PersistPersonInfo(tenantUnitOfWorkManager);

			tenant = new Tenant(RandomName.Make());
			tenantUnitOfWorkManager.CurrentSession().Save(tenant);
		}

		[TearDown]
		public void Cleanup()
		{
			tenantUnitOfWorkManager.Dispose();
		}

		[Test]
		public void ShouldInsertPersonInfo()
		{
			var session = tenantUnitOfWorkManager.CurrentSession(); 
			
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
			var session = tenantUnitOfWorkManager.CurrentSession();

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

			var session = tenantUnitOfWorkManager.CurrentSession();

			var id = Guid.NewGuid();

			var personInfo = new PersonInfo(tenant, id);
			session.Save(personInfo);
			session.Flush();
			session.Clear();

			personInfo.SetApplicationLogonCredentials(new CheckPasswordStrengthSuccessful(), RandomName.Make(), newPassword);
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
			personInfo1.SetApplicationLogonCredentials(new CheckPasswordStrengthSuccessful(), logonName, RandomName.Make());
			var personInfo2 = new PersonInfo(tenant, Guid.NewGuid());
			personInfo2.SetApplicationLogonCredentials(new CheckPasswordStrengthSuccessful(), logonName, RandomName.Make());

			target.Persist(personInfo1);
			target.Persist(personInfo2);

			Assert.Throws(Is.TypeOf<GenericADOException>().And.InnerException.Message.Contains("Cannot insert duplicate"),
				tenantUnitOfWorkManager.CurrentSession().Flush);
		}

		[Test]
		public void MultipleNullApplicationLogonShouldNotThrow()
		{
			var personInfo1 = new PersonInfo(tenant, Guid.NewGuid());
			personInfo1.SetApplicationLogonCredentials(new CheckPasswordStrengthSuccessful(), null, RandomName.Make());
			var personInfo2 = new PersonInfo(tenant, Guid.NewGuid());
			personInfo2.SetApplicationLogonCredentials(new CheckPasswordStrengthSuccessful(), null, RandomName.Make());

			target.Persist(personInfo1);
			target.Persist(personInfo2);

			Assert.DoesNotThrow(tenantUnitOfWorkManager.CurrentSession().Flush);
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

			Assert.Throws(Is.TypeOf<GenericADOException>().And.InnerException.Message.Contains("Cannot insert duplicate"),
				tenantUnitOfWorkManager.CurrentSession().Flush);
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

			Assert.DoesNotThrow(tenantUnitOfWorkManager.CurrentSession().Flush);
		}
	}
}