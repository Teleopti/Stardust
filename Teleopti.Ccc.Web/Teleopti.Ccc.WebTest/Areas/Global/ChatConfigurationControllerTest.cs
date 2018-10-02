﻿using System.Net;
using System.Net.Http;
using Newtonsoft.Json;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.IoC;
using Teleopti.Ccc.Web.Areas.Global;

namespace Teleopti.Ccc.WebTest.Areas.Global
{
	[DomainTest]
	public class ChatConfigurationControllerTest : IIsolateSystem
	{
		public ChatConfigurationController Target;
		public FakeHttpServer HttpServer;

		[Test]
		public void ShouldCheckIfBotIsConfigured()
		{
			HttpServer.FakeResponseMessage(new HttpResponseMessage(HttpStatusCode.OK) { Content = new StringContent(JsonConvert.SerializeObject(true))});

			Target.Exists(new TenantCredential{Host = "localhost:52858", Tenant = "Teleopti WFM"}).Result.Should().Be.True();
		}

		[Test]
		public void ShouldConfigureBot()
		{
			var tenantCredential = new TenantCredential {Host = "localhost:52858", Tenant = "Teleopti WFM", ApiKey = "topsecretkeygoeshere"};
			Target.Configure(tenantCredential);

			HttpServer.Requests[0].Thing.Should().Be.SameInstanceAs(tenantCredential);
		}

		public void Isolate(IIsolate isolate)
		{
			isolate.UseTestDouble<ChatConfigurationController>().For<ChatConfigurationController>();
			isolate.UseTestDouble<FakeHttpServer>().For<IHttpServer>();
		}
	}
}
