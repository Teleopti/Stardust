using System.Security.Principal;
using System.Web;
using NUnit.Framework;
using Rhino.Mocks;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Ccc.Infrastructure.Foundation;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.TestCommon.Web;
using Teleopti.Ccc.Web.Core.RequestContext;
using Teleopti.Ccc.Web.Core.RequestContext.Initialize;

namespace Teleopti.Ccc.WebTest.Core.RequestContext
{
	[TestFixture]
	public class RequestContextInitializerTest
	{
		private RequestContextInitializer _requestContextInitializer;
		private HttpContextBase _httpContextBase;
		private TeleoptiPrincipalForLegacy _teleoptiPrincipal;
	    private ISetThreadCulture _setThreadCulture;

	    [SetUp]
		public void SetUp()
	    {
		    var person = PersonFactory.CreatePerson ("Test Person");
			_httpContextBase = MockRepository.GenerateStub<HttpContextBase>();
		    _setThreadCulture = MockRepository.GenerateMock<ISetThreadCulture>();
			_teleoptiPrincipal = new TeleoptiPrincipalForLegacy(new GenericIdentity("MyName"), person);

		    _requestContextInitializer = new RequestContextInitializer(
			    new TestSessionPrincipalFactory(_teleoptiPrincipal),
				    _setThreadCulture,
				    new WebRequestPrincipalContext(
					    new FakeCurrentHttpContext(_httpContextBase),
						    new ThreadPrincipalContext()
					    )
			    );
	    }

		[Test]
		public void ShouldAttachPrincipalToCurrentContext()
		{
			_setThreadCulture.Expect(t => t.SetCulture(_teleoptiPrincipal.Regional));
		    _setThreadCulture.Replay();

            _requestContextInitializer.SetupPrincipalAndCulture(false);
                _httpContextBase.User.Should().Be.SameInstanceAs(_teleoptiPrincipal);

            _setThreadCulture.VerifyAllExpectations();
		}

		protected class TestSessionPrincipalFactory : ISessionPrincipalFactory
		{
			public TestSessionPrincipalFactory(ITeleoptiPrincipal principal)
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