using System.Collections.Specialized;
using System.Web;
using NUnit.Framework;
using Rhino.Mocks;
using SharpTestsEx;
using Teleopti.Ccc.Web.Areas.Start.Core.Authentication.DataProvider;

namespace Teleopti.Ccc.WebTest.Core.Authentication.DataProvider
{
	//todo: Gillar inte mockeridjupet på httpcontextbase. Finns säkert nåt lib därute som gör detta enklare
	//kolla upp det...
	[TestFixture]
	public class WindowsAccountProviderTest
	{
		private WindowsAccountProvider target;

		private HttpRequestBase httpRequest;
		private HttpContextBase httpContext;
		private NameValueCollection serverVariables;

		[SetUp]
		public void Setup()
		{
			//mocks = new MockRepository();
			//http = mocks.DynamicMock<HttpContextBase>();

			serverVariables = new NameValueCollection();
			httpRequest = MockRepository.GenerateStub<HttpRequestBase>();
			httpRequest.Stub(x => x.ServerVariables).Return(serverVariables);

			httpContext = MockRepository.GenerateStub<HttpContextBase>();
			httpContext.Stub(x => x.Request).Return(httpRequest);

			target = new WindowsAccountProvider(httpContext); 
		}

		[Test]
		public void ShouldSplitNameIntoUserNameAndDomainName()
		{
			const string fakeDomainName = "domain Name";
			const string fakeUserName = " le faking userNAME";
			const string userName = fakeDomainName + @"\" + fakeUserName;
			
			serverVariables.Add("LOGON_USER", userName);

			target.RetrieveWindowsAccount().DomainName
				.Should().Be.EqualTo(fakeDomainName);
			target.RetrieveWindowsAccount().UserName
				.Should().Be.EqualTo(fakeUserName);

		}
		[Test]
		public void ShouldReturnNullIfNotInDomain()
		{
			string userName = string.Empty;
			serverVariables.Add("LOGON_USER", userName);

			WindowsAccount retrieveWindowsAccount = target.RetrieveWindowsAccount();

			retrieveWindowsAccount.Should().Be.Null();
			

		}

	}
}