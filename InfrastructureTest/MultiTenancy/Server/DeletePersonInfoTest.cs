using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Infrastructure.MultiTenancy.Server;
using Teleopti.Ccc.Infrastructure.MultiTenancy.Server.NHibernate;
using Teleopti.Ccc.TestCommon;

namespace Teleopti.Ccc.InfrastructureTest.MultiTenancy.Server
{
	public class DeletePersonInfoTest
	{
		private TenantUnitOfWorkManager uowManager;
		private Tenant tenant;
		private PersonInfo person;

		[Test]
		public void ShouldDeletePerson()
		{
			var target = new DeletePersonInfo(uowManager);
			target.Delete(person);

			var session = uowManager.CurrentSession();
			session.Flush();
			session.Get<PersonInfo>(person.Id)
				.Should().Be.Null();
		}

		[SetUp]
		public void Setup()
		{
			uowManager = TenantUnitOfWorkManager.CreateInstanceForTest(ConnectionStringHelper.ConnectionStringUsedInTests);
			var session = uowManager.CurrentSession();
			tenant = new Tenant("som name");
			person = new PersonInfo(tenant);
			session.Save(tenant);
			session.Save(person);
			session.Flush();
		}

		[TearDown]
		public void TearDown()
		{
			uowManager.Dispose();
		}
	}
}