using System.Collections.Specialized;
using System.Web;
using NUnit.Framework;
using Rhino.Mocks;
using SharpTestsEx;
using Teleopti.Ccc.Web.WindowsIdentityProvider.Core;

namespace Teleopti.Ccc.Web.WindowsIdentityProviderTest.Core
{
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