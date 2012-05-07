using System.Security.Principal;
using System.Web;
using NUnit.Framework;
using Rhino.Mocks;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Ccc.Web.Core.RequestContext;

namespace Teleopti.Ccc.WebTest.Core.RequestContext
{
	[TestFixture]
	public class RequestContextInitializerTest
	{
		private RequestContextInitializer _requestContextInitializer;
		private HttpContextBase _httpContextBase;
		private TeleoptiPrincipal _teleoptiPrincipal;
	    private ISetThreadCulture _setThreadCulture;

	    [SetUp]
		public void SetUp()
		{
			_httpContextBase = MockRepository.GenerateStub<HttpContextBase>();
		    _setThreadCulture = MockRepository.GenerateMock<ISetThreadCulture>();
			_teleoptiPrincipal = new TeleoptiPrincipal(new GenericIdentity("MyName"), null);
			_requestContextInitializer = new RequestContextInitializer(_httpContextBase,
			                                                           new TestPrincipalFactory(_teleoptiPrincipal), _setThreadCulture);
		}

		[Test]
		public void ShouldAttachPrincipalToCurrentContext()
		{
		    _setThreadCulture.Expect(t => t.SetCulture(null));
		    _setThreadCulture.Replay();

            _requestContextInitializer.AttachPrincipalForAuthenticatedUser();
                _httpContextBase.User.Should().Be.SameInstanceAs(_teleoptiPrincipal);

            _setThreadCulture.VerifyAllExpectations();
		}

		protected class TestPrincipalFactory : IPrincipalFactory
		{
			public TestPrincipalFactory(ITeleoptiPrincipal principal)
			{
				_principal = principal;
			}

			private readonly ITeleoptiPrincipal _principal;

			public ITeleoptiPrincipal Generate()
			{
				return _principal;
			}
		}
	}
}