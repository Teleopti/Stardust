using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Infrastructure.MultiTenancy.Admin;
using Teleopti.Ccc.Infrastructure.MultiTenancy.Server.NHibernate;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.TestData;


namespace Teleopti.Ccc.InfrastructureTest.MultiTenancy.Admin
{
	public class FindTenantAdminUserByEmailTest
	{
		private TenantUnitOfWorkManager tenantUnitOfWorkManager;

		[Test]
		public void ShouldFindUser()
		{
			var email = RandomName.Make();
			var user = new TenantAdminUser {Email = email, Name = RandomName.Make(), Password = RandomName.Make(), AccessToken = RandomName.Make()};
			tenantUnitOfWorkManager.CurrentSession().Save(user);

			new FindTenantAdminUserByEmail(tenantUnitOfWorkManager).Find(email).Id
				.Should().Be.EqualTo(user.Id);
		}

		[Test]
		public void ShouldReturnNullIfUserDoesntExist()
		{
			new FindTenantAdminUserByEmail(tenantUnitOfWorkManager).Find(RandomName.Make())
				.Should().Be.Null();
		}

		[SetUp]
		public void Setup()
		{
			tenantUnitOfWorkManager = TenantUnitOfWorkManager.Create(InfraTestConfigReader.ConnectionString);
			tenantUnitOfWorkManager.EnsureUnitOfWorkIsStarted();
		}

		[TearDown]
		public void RollbackTransaction()
		{
			tenantUnitOfWorkManager.Dispose();
		}
	}
}