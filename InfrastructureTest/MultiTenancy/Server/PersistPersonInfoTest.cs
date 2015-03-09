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

		[Test]
		public void ShouldInsertPersonInfo()
		{
			var session = tenantUnitOfWorkManager.CurrentSession(); 
			
			var personInfo = new PersonInfo(tenant);
			target.Persist(personInfo);

			session.Flush();
			session.Clear();
			session.Get<PersonInfo>(personInfo.Id).Should().Not.Be.Null();
		}

		[Test]
		public void ShouldUpdatePersonInfo()
		{
			var newPassword = RandomName.Make();

			var session = tenantUnitOfWorkManager.CurrentSession();

			var personInfo = new PersonInfo(tenant);
			session.Save(personInfo);
			var idBefore = personInfo.Id;
			session.Flush();
			session.Clear();

			personInfo.Password = newPassword;
			target.Persist(personInfo);

			session.Flush();
			session.Clear();
			var loaded = session.Get<PersonInfo>(personInfo.Id);
			loaded.Password.Should().Be.EqualTo(newPassword);
			loaded.Id.Should().Be.EqualTo(idBefore);
		}

		[Test]
		public void SameApplicationLogonShouldThrow()
		{
			var logonName = RandomName.Make();

			var personInfo1 = new PersonInfo(tenant);
			personInfo1.SetApplicationLogonName(logonName);
			var personInfo2 = new PersonInfo(tenant);
			personInfo2.SetApplicationLogonName(logonName);

			target.Persist(personInfo1);
			target.Persist(personInfo2);

			Assert.Throws(Is.TypeOf<GenericADOException>().And.InnerException.Message.Contains("Cannot insert duplicate"),
				tenantUnitOfWorkManager.CurrentSession().Flush);
		}

		[Test]
		public void MultipleNullApplicationLogonShouldNotThrow()
		{
			var personInfo1 = new PersonInfo(tenant);
			personInfo1.SetApplicationLogonName(null);
			var personInfo2 = new PersonInfo(tenant);
			personInfo2.SetApplicationLogonName(null);

			target.Persist(personInfo1);
			target.Persist(personInfo2);

			Assert.DoesNotThrow(tenantUnitOfWorkManager.CurrentSession().Flush);
		}

		[Test]
		public void SameIdentityShouldThrow()
		{
			var identity = RandomName.Make();

			var personInfo1 = new PersonInfo(tenant);
			personInfo1.SetIdentity(identity);
			var personInfo2 = new PersonInfo(tenant);
			personInfo2.SetIdentity(identity);

			target.Persist(personInfo1);
			target.Persist(personInfo2);

			Assert.Throws(Is.TypeOf<GenericADOException>().And.InnerException.Message.Contains("Cannot insert duplicate"),
				tenantUnitOfWorkManager.CurrentSession().Flush);
		}

		public void MultipleNullIdentityShouldNotThrow()
		{
			var personInfo1 = new PersonInfo(tenant);
			personInfo1.SetIdentity(null);
			var personInfo2 = new PersonInfo(tenant);
			personInfo2.SetIdentity(null);

			target.Persist(personInfo1);
			target.Persist(personInfo2);

			Assert.DoesNotThrow(tenantUnitOfWorkManager.CurrentSession().Flush);
		}
	}
}