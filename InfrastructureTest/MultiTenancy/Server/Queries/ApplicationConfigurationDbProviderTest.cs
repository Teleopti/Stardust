using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Config;
using Teleopti.Ccc.Domain.MultiTenancy;
using Teleopti.Ccc.Infrastructure.MultiTenancy.Admin;
using Teleopti.Ccc.Infrastructure.MultiTenancy.Server;
using Teleopti.Ccc.Infrastructure.MultiTenancy.Server.NHibernate;
using Teleopti.Ccc.Infrastructure.MultiTenancy.Server.Queries;
using Teleopti.Ccc.Infrastructure.Web;
using Teleopti.Ccc.InfrastructureTest.Helper;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.Web;

namespace Teleopti.Ccc.InfrastructureTest.MultiTenancy.Server
{
	[TestFixture]
	public class ApplicationConfigurationDbProviderTest : DatabaseTest
	{
		private TenantUnitOfWorkManager tenantUnitOfWorkManager;
		private PersonInfo personInfo;
		private ApplicationConfigurationDbProvider Target;
		private FakeHttpContext fakeContext;
		public CurrentHttpContext HttpContext;
		public PersistTenant Persist;
		

		protected override void SetupForRepositoryTest()
		{
			base.SetupForRepositoryTest();
			tenantUnitOfWorkManager = TenantUnitOfWorkManager.Create(InfraTestConfigReader.ApplicationConnectionString());
			tenantUnitOfWorkManager.EnsureUnitOfWorkIsStarted();
			personInfo = new PersonInfo();
			fakeContext = new FakeHttpContext();
			fakeContext.Items.Add(WebTenantAuthenticationConfiguration.PersonInfoKey, personInfo);
			HttpContext = new CurrentHttpContext();
			Persist = new PersistTenant(tenantUnitOfWorkManager);

			Target = new ApplicationConfigurationDbProvider(new ServerConfigurationRepository(tenantUnitOfWorkManager),
				new CurrentTenant(new CurrentTenantUser(new CurrentHttpContext())));
		}

		[TearDown]
		public void RollbackTransaction()
		{
			tenantUnitOfWorkManager.Dispose();
		}

		[Test]
		public void ShouldGetAllConfigurationData()
		{
			using (HttpContext.OnThisThreadUse(fakeContext))
			{
				var appConfig = Target.GetAll();

				var serverKey = ServerConfigurationKey.NotificationContentType;
				var tenantKey = TenantApplicationConfigKey.NotificationApiKey;
				var serverRepo = new ServerConfigurationRepository(tenantUnitOfWorkManager);
				serverRepo.Update(serverKey.ToString(), "ServerValue");
				personInfo.Tenant.SetApplicationConfig(tenantKey, "TenantValue");
				Persist.Persist(personInfo.Tenant);
				tenantUnitOfWorkManager.CurrentSession().Flush();

				var acAfterUpdate = Target.GetAll();

				Assert.AreEqual(acAfterUpdate.Server.Count, appConfig.Server.Count + 1);
				Assert.AreEqual(acAfterUpdate.Tenant.Count, appConfig.Tenant.Count + 1);
				acAfterUpdate.Server.ContainsKey(serverKey).Should().Be.True();
				acAfterUpdate.Tenant.ContainsKey(tenantKey).Should().Be.True();
			}
		}

		[Test]
		public void ShouldGetCorrectConfigurationData()
		{
			using (HttpContext.OnThisThreadUse(fakeContext))
			{
				var server = new {Key = ServerConfigurationKey.NotificationApiEndpoint, Value = "ServerValue"};
				var tenant = new {Key = TenantApplicationConfigKey.NotificationApiKey, Value = "TenantValue"};
				var serverRepo = new ServerConfigurationRepository(tenantUnitOfWorkManager);
				serverRepo.Update(server.Key.ToString(), server.Value);
				personInfo.Tenant.SetApplicationConfig(tenant.Key, tenant.Value);
				Persist.Persist(personInfo.Tenant);
				tenantUnitOfWorkManager.CurrentSession().Flush();

				var serverRead = Target.GetServerValue(server.Key);
				var tenantRead = Target.GetTenantValue(tenant.Key);

				Assert.AreEqual(serverRead, server.Value);
				Assert.AreEqual(tenantRead, tenant.Value);
			}
		}
	}
}