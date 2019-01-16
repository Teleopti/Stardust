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
			var serverData = Target.TryGetServerValue(ServerConfigurationKey.NotificationSmtpPort.ToString(), null).Result<string>();
			Assert.AreEqual(serverData, _fakeConfig.Server[ServerConfigurationKey.NotificationSmtpPort.ToString()]);
		
			var tenantData = Target.TryGetTenantValue(TenantApplicationConfigKey.NotificationApiKey.ToString(), null).Result<string>();
			Assert.AreEqual(tenantData, _fakeConfig.Tenant[TenantApplicationConfigKey.NotificationApiKey.ToString()]);
		}

		[Test]
		public void ShouldReturDefaultValueIfKeyIsMissing()
		{
			ConfigDbProvider.LoadFakeData(_fakeConfig);
			var defVal = "DEFAULT";
			var tenantData = Target.TryGetTenantValue(TenantApplicationConfigKey.MaximumSessionTimeInMinutes.ToString(), defVal).Result<string>();
			Assert.AreEqual(tenantData, defVal);

			tenantData = Target.TryGetTenantValue("RandomKey", defVal).Result<string>();
			Assert.AreEqual(tenantData, defVal);

			var serverData = Target.TryGetServerValue(ServerConfigurationKey.AS_DATABASE.ToString(), defVal).Result<string>();
			Assert.AreEqual(serverData, defVal);

			serverData = Target.TryGetServerValue("RandomKey", defVal).Result<string>();
			Assert.AreEqual(serverData, defVal);
		}
	}
}