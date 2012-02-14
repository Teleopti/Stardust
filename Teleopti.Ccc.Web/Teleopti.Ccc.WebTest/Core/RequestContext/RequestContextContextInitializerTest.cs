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
	public class RequestContextContextInitializerTest
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
			                                                           new TestPrincipalProvider(_teleoptiPrincipal), _setThreadCulture);
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

		protected class TestPrincipalProvider : IPrincipalProvider
		{
			public TestPrincipalProvider(TeleoptiPrincipal principal)
			{
				_principal = principal;
			}

			private readonly TeleoptiPrincipal _principal;

			public TeleoptiPrincipal Generate()
			{
				return _principal;
			}
		}
	}
}