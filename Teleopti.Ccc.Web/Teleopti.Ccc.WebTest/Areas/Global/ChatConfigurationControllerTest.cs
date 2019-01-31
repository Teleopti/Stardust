using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Domain.MultiTenancy;
using Teleopti.Ccc.Infrastructure.MultiTenancy.Admin;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeRepositories;
using Teleopti.Ccc.TestCommon.IoC;
using Teleopti.Ccc.Web.Areas.Global;

namespace Teleopti.Ccc.WebTest.Areas.Global
{
	[DomainTest]
	public class ChatConfigurationControllerTest : IIsolateSystem
	{
		public ChatConfigurationController Target;
		public FakeHttpServer HttpServer;
		public FakeServerConfigurationRepository ServerConfigurationRepository;
		public FakeConfigReader ConfigReader;

		[Test]
		public void ShouldCheckIfBotIsConfigured()
		{
			ConfigReader.FakeSetting(ServerConfigurationKey.GrantBotApiUrl.ToString(),"test");
			HttpServer.FakeResponseMessage(new HttpResponseMessage(HttpStatusCode.OK) { Content = new StringContent(JsonConvert.SerializeObject(true))});

			Target.Exists(new TenantCredential{Host = "localhost:52858", Tenant = "Teleopti WFM"}).GetAwaiter().GetResult().Should().Be.True();
			HttpServer.Requests[0].Uri.Should().Contain("test");
		}

		[Test]
		public void ShouldCheckIfBotIsConfiguredUsingServerSettings()
		{
			ConfigReader.FakeSetting(ServerConfigurationKey.GrantBotApiUrl.ToString(), "test");
			ServerConfigurationRepository.Update(ServerConfigurationKey.GrantBotApiUrl.ToString(), "myurl");
			HttpServer.FakeResponseMessage(new HttpResponseMessage(HttpStatusCode.OK) { Content = new StringContent(JsonConvert.SerializeObject(true))});

			Target.Exists(new TenantCredential{Host = "localhost:52858", Tenant = "Teleopti WFM"}).GetAwaiter().GetResult().Should().Be.True();

			HttpServer.Requests[0].Uri.Should().Contain("myurl");
		}

		[Test]
		public async Task ShouldConfigureBotUsingAppConfig()
		{
			ConfigReader.FakeSetting(ServerConfigurationKey.GrantBotApiUrl.ToString(), "test");

			var tenantCredential = new TenantCredential {Host = "localhost:52858", Tenant = "Teleopti WFM", ApiKey = "topsecretkeygoeshere"};
			await Target.Configure(tenantCredential);

			HttpServer.Requests[0].Data.Should().Be.SameInstanceAs(tenantCredential);
			HttpServer.Requests[0].Uri.Should().Be.EqualTo("test");
		}

		[Test]
		public async Task ShouldConfigureBotUsingServerSettings()
		{
			ConfigReader.FakeSetting(ServerConfigurationKey.GrantBotApiUrl.ToString(), "test");
			ServerConfigurationRepository.Update(ServerConfigurationKey.GrantBotApiUrl.ToString(), "myurl");

			var tenantCredential = new TenantCredential {Host = "localhost:52858", Tenant = "Teleopti WFM", ApiKey = "topsecretkeygoeshere"};
			await Target.Configure(tenantCredential);

			HttpServer.Requests[0].Data.Should().Be.SameInstanceAs(tenantCredential);
			HttpServer.Requests[0].Uri.Should().Be.EqualTo("myurl");
		}

		public void Isolate(IIsolate isolate)
		{
			isolate.UseTestDouble<ChatConfigurationController>().For<ChatConfigurationController>();
			isolate.UseTestDouble<FakeHttpServer>().For<IHttpServer>();
			isolate.UseTestDouble<FakeServerConfigurationRepository>().For<IServerConfigurationRepository>();
		}
	}
}
