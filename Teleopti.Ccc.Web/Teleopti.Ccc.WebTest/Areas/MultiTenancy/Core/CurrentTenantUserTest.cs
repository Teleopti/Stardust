using NUnit.Framework;
using Rhino.Mocks;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Config;
using Teleopti.Ccc.Infrastructure.MultiTenancy.Server;
using Teleopti.Ccc.TestCommon.Web;
using Teleopti.Ccc.Web.Core.RequestContext.Cookie;

namespace Teleopti.Ccc.WebTest.Areas.MultiTenancy.Core
{
	public class CurrentTenantUserTest
	{
		[Test]
		public void ShouldReturnNullIfNotExist()
		{
			var sessionDataProvider = MockRepository.GenerateStub<ISessionSpecificWfmCookieProvider>();
			sessionDataProvider.Expect(x => x.GrabFromCookie()).Return(null);
			new CurrentTenantUser(new FakeCurrentHttpContext(new FakeHttpContext())).CurrentUser()
				.Should().Be.Null();
		}

		[Test]
		public void ShouldGetPersonInfo()
		{
			var expected = new PersonInfo();
			var httpContext = new FakeHttpContext();
			httpContext.Items[WebTenantAuthenticationConfiguration.PersonInfoKey] = expected;

			new CurrentTenantUser(new FakeCurrentHttpContext(httpContext)).CurrentUser()
					.Should().Be.SameInstanceAs(expected);
		}
	}
}