using NUnit.Framework;
using System.Collections.Generic;
using Teleopti.Ccc.Domain.MultiTenancy;
using Teleopti.Ccc.Infrastructure.MultiTenancy.Server;
using Teleopti.Ccc.Web.Areas.MultiTenancy;
using Teleopti.Ccc.WebTest.TestHelper;

namespace Teleopti.Ccc.WebTest.Areas.MultiTenancy
{
	[TenantTest]
	public class ApplicationConfigurationDbControllerTest
	{
		public ApplicationConfigurationDbController Target;
		public ApplicationConfigurationDbProviderFake ConfigDbProvider;
		private ApplicationConfigurationDb _fakeConfig;

		[SetUp]
		public void SetUp()
		{
			_fakeConfig = new ApplicationConfigurationDb
			{
				Server = new Dictionary<ServerConfigurationKey, string>
				{
					{ ServerConfigurationKey.NotificationApiEndpoint, "http://api.teleopti.com" },
					{ ServerConfigurationKey.NotificationSmtpPort, "25" }
				},
				Tenant = new Dictionary<TenantApplicationConfigKey, string>
				{
					{ TenantApplicationConfigKey.NotificationApiKey, "<key>"},
					{ TenantApplicationConfigKey.MobileQRCodeUrl, "http://www.teleopti.com"}
				}
			};
		}

		[Test]
		public void ShouldGetAllConfigurationData()
		{
			ConfigDbProvider.LoadFakeData(_fakeConfig);

			var configData = Target.GetAll().Result<ApplicationConfigurationDb>();
			Assert.IsTrue(configData.Tenant.ContainsKey(TenantApplicationConfigKey.NotificationApiKey));
			Assert.IsTrue(configData.Tenant.ContainsKey(TenantApplicationConfigKey.MobileQRCodeUrl));
			Assert.IsTrue(configData.Server.ContainsKey(ServerConfigurationKey.NotificationApiEndpoint));
			Assert.IsTrue(configData.Server.ContainsKey(ServerConfigurationKey.NotificationSmtpPort));
			Assert.AreEqual(configData.Server.Count, 2);
			Assert.AreEqual(configData.Tenant.Count, 2);
		}

		[Test]
		public void ShouldGetConfigurationDataByKey()
		{
			ConfigDbProvider.LoadFakeData(_fakeConfig);
			var serverData = Target.GetServerValue(ServerConfigurationKey.NotificationSmtpPort.ToString()).Result<string>();
			Assert.AreEqual(serverData, _fakeConfig.Server[ServerConfigurationKey.NotificationSmtpPort]);
		
			var tenantData = Target.GetTenantValue(TenantApplicationConfigKey.NotificationApiKey.ToString()).Result<string>();
			Assert.AreEqual(tenantData, _fakeConfig.Tenant[TenantApplicationConfigKey.NotificationApiKey]);
		}

		[Test]
		public void ShouldReturNullIfKeyIsMissing()
		{
			ConfigDbProvider.LoadFakeData(_fakeConfig);
			var tenantData = Target.GetTenantValue(TenantApplicationConfigKey.MaximumSessionTimeInMinutes.ToString()).Result<string>();
			Assert.AreEqual(tenantData, null);

			tenantData = Target.GetTenantValue("RandomKey").Result<string>();
			Assert.AreEqual(tenantData, null);

			var serverData = Target.GetServerValue(ServerConfigurationKey.AS_DATABASE.ToString()).Result<string>();
			Assert.AreEqual(serverData, null);

			serverData = Target.GetServerValue("RandomKey").Result<string>();
			Assert.AreEqual(serverData, null);
		}
	}
}