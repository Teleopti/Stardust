using System.Web;
using System.Web.Routing;
using NUnit.Framework;
using Teleopti.Ccc.TestCommon.Web;
using Teleopti.Ccc.Web.Areas.Start.Controllers;

namespace Teleopti.Ccc.WebTest.Areas.Start.Core
{
	[TestFixture]
	class WebApplicationAreaResolverTest
	{


		[Test]
		public void ShouldCorrectlyRetrieveAreaFromDevelopmentUrl()
		{
			const string url = "http://localhost:52858/Start/Return/Hash?redirectUrl=http%3A%2F%2Flocalhost%3A52857%2F%3Fwa%3Dwsignin1.0%26wtrealm%3Dhttp%253a%252f%252fsample-with-policyengine%252f%26wctx%3Dru%253d%252fMyTime%252fCiscoWidget%26whr%3Durn%253aTeleopti";
		
			Assert.AreEqual("MYTIME/CISCOWIDGET", getWebApplicationArea(url, "/"));
		}

		[Test]
		public void ShouldCorrectlyRetrieveAreaFromDeploymentUrl()
		{
			const string url ="http://localhost/TeleoptiWFM/Web/Start/Return/Hash?redirectUrl=%2FTeleoptiWFM%2FAuthenticationBridge%2F%3Fwa%3Dwsignin1.0%26wtrealm%3Dhttp%253a%252f%252fsample-with-policyengine%252f%26wctx%3Dru%253d%252fTeleoptiWFM%252fWeb%252fmytime%252fCiscoWidget%26whr%3Durn%253aWindows";
			
			Assert.AreEqual("MYTIME/CISCOWIDGET", getWebApplicationArea(url, "/TeleoptiWFM/Web/"));
		}

		private static string getWebApplicationArea (string url, string applicationRelativeUrl)
		{
			var httpRequest = new HttpRequestWrapper(new HttpRequest("", url, getQueryString(url)))
			{
				RequestContext = new RequestContext (new FakeHttpContext(), new RouteData())
			};

			return WebApplicationAreaResolver.GetWebApplicationArea (httpRequest, applicationRelativeUrl).ToUpper();
			
		}

		private static string getQueryString (string url)
		{
			var idxOfRedirectUrl = url.IndexOf ("redirectUrl");
			var queryString = url.Substring (idxOfRedirectUrl);
			return queryString;
		}
	}
}
