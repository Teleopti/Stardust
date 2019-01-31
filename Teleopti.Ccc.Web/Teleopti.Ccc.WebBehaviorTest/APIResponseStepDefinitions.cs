using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using NUnit.Framework;
using TechTalk.SpecFlow;
using Teleopti.Ccc.TestCommon.Web.WebInteractions;

namespace Teleopti.Ccc.WebBehaviorTest
{
	[Binding]
	public sealed class ApiResponseStepDefinition
	{
		private readonly HttpClient client = new HttpClient();

		public string ResponseServerVersionHeader;
		public HttpStatusCode ResponseStatusCode;

		[Given("Webclient requests a missing api endpoint")]
		public void WebclientDoesRequest()
		{
				var response = client.GetAsync(new Uri(TestSiteConfigurationSetup.URL, "whywouldanyonequerythis")).Result;
				ResponseStatusCode = response.StatusCode;
				ResponseServerVersionHeader = response.Headers.GetValues("X-Server-Version").FirstOrDefault();
		}

		[Then("The response should contain status 404 and X-Server-Version header")]
		public void RequestCheck()
		{
			var serverVersion = ResponseServerVersionHeader;
			var statusCode = ResponseStatusCode;
			Assert.IsNotNull(serverVersion);
			Assert.AreEqual(HttpStatusCode.NotFound, statusCode);
		}
	}
}