using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using NUnit.Framework;
using TechTalk.SpecFlow;
using TechTalk.SpecFlow.Assist;
using Teleopti.Ccc.Infrastructure.MultiTenancy.Server;
using Teleopti.Ccc.Infrastructure.MultiTenancy.Server.Queries;
using Teleopti.Ccc.Infrastructure.Security;
using Teleopti.Ccc.Infrastructure.UnitOfWork;
using Teleopti.Ccc.TestCommon.TestData.Setups.Configurable;
using Teleopti.Ccc.TestCommon.Web.WebInteractions;
using Teleopti.Ccc.WebBehaviorTest.Core;
using Teleopti.Ccc.WebBehaviorTest.Data;
using Teleopti.Ccc.WebBehaviorTest.Data.Setups.Configurable;
using PersonPeriodConfigurable = Teleopti.Ccc.WebBehaviorTest.Data.Setups.Configurable.PersonPeriodConfigurable;

namespace Teleopti.Ccc.WebBehaviorTest
{
	[Binding]
	public sealed class ApiResponseStepDefinition
	{
		public HttpWebResponse Response;

		[Given("Webclient requests a missing api endpoint")]
		public void WebclientDoesRequest()
		{
			WebRequest request = WebRequest.Create(new Uri(TestSiteConfigurationSetup.URL, "whywouldanyonequerythis"));
			try
			{
				Response = (HttpWebResponse) request.GetResponse();
			}
			catch (WebException we)
			{
				Response = we.Response as HttpWebResponse;
			}
		}

		[Then("The response should contain status 404 and X-Server-Version header")]
		public void RequestCheck()
		{
			var serverVersion = Response.Headers.Get("X-Server-Version");
			var statusCode = Response.StatusCode;
			Assert.IsNotNull(serverVersion);
			Assert.AreSame(HttpStatusCode.NotFound, statusCode);
		}
	}
}