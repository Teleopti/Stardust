using System.Collections.Specialized;
using System.Web;
using NUnit.Framework;
using Rhino.Mocks;
using SharpTestsEx;
using Teleopti.Ccc.Web.Areas.SSO.Core;
using Teleopti.Ccc.Web.Core.RequestContext;

namespace Teleopti.Ccc.WebTest.Areas.SSO.Core
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

			var currentHttpContext = MockRepository.GenerateMock<ICurrentHttpContext>();
			currentHttpContext.Stub(x => x.Current()).Return(httpContext);

			target = new WindowsAccountProvider(currentHttpContext); 
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

			var retrieveWindowsAccount = target.RetrieveWindowsAccount();

			retrieveWindowsAccount.Should().Be.Null();
			

		}

	}
}