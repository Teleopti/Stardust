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
				Server = new Dictionary<string, string>
				{
					{ ServerConfigurationKey.NotificationApiEndpoint.ToString(), "http://api.teleopti.com" },
					{ ServerConfigurationKey.NotificationSmtpPort.ToString(), "25" }
				},
				Tenant = new Dictionary<string, string>
				{
					{ TenantApplicationConfigKey.NotificationApiKey.ToString(), "<key>"},
					{ TenantApplicationConfigKey.MobileQRCodeUrl.ToString(), "http://www.teleopti.com"}
				}
			};
		}

		[Test]
		public void ShouldGetAllConfigurationData()
		{
			ConfigDbProvider.LoadFakeData(_fakeConfig);

			var configData = Target.GetAll().Result<ApplicationConfigurationDb>();
			Assert.IsTrue(configData.Tenant.ContainsKey(TenantApplicationConfigKey.NotificationApiKey.ToString()));
			Assert.IsTrue(configData.Tenant.ContainsKey(TenantApplicationConfigKey.MobileQRCodeUrl.ToString()));
			Assert.IsTrue(configData.Server.ContainsKey(ServerConfigurationKey.NotificationApiEndpoint.ToString()));
			Assert.IsTrue(configData.Server.ContainsKey(ServerConfigurationKey.NotificationSmtpPort.ToString()));
			Assert.AreEqual(configData.Server.Count, 2);
			Assert.AreEqual(configData.Tenant.Count, 2);
		}

		[Test]
		public void ShouldGetConfigurationDataByKey()
		{
			ConfigDbProvider.LoadFakeData(_fakeConfig);
			var serverData = Target.GetServerValue(ServerConfigurationKey.NotificationSmtpPort.ToString()).Result<string>();
			Assert.AreEqual(serverData, _fakeConfig.Server[ServerConfigurationKey.NotificationSmtpPort.ToString()]);
		
			var tenantData = Target.GetTenantValue(TenantApplicationConfigKey.NotificationApiKey.ToString()).Result<string>();
			Assert.AreEqual(tenantData, _fakeConfig.Tenant[TenantApplicationConfigKey.NotificationApiKey.ToString()]);
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