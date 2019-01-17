using System;
using System.Collections.Specialized;
using System.Web;
using NUnit.Framework;
using Rhino.Mocks;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Config;
using Teleopti.Ccc.Infrastructure.MultiTenancy.Server;
using Teleopti.Ccc.Infrastructure.MultiTenancy.Server.Queries;
using Teleopti.Ccc.TestCommon.TestData;
using Teleopti.Ccc.TestCommon.Web;
using Teleopti.Ccc.Web.Areas.MultiTenancy.Core;
using Teleopti.Ccc.Web.Core.RequestContext.Cookie;

namespace Teleopti.Ccc.WebTest.Areas.MultiTenancy.Core
{
	public class WebTenantAuthenticationTest
	{
		[Test]
		public void ShouldSetPersonInfoIfSuccessful_Web()
		{
			var personId = Guid.NewGuid();
			var tenantPassword = RandomName.Make();
			var expected = new PersonInfo();
			var httpcontext = new FakeHttpContext();

			var sessionDataProvider = MockRepository.GenerateMock<ISessionSpecificWfmCookieProvider>();
			sessionDataProvider.Expect(x => x.GrabFromCookie()).Return(new SessionSpecificData(Guid.NewGuid(), RandomName.Make(), personId, tenantPassword, false));
			var findPersonByCredentials = MockRepository.GenerateMock<IFindPersonInfoByCredentials>();
			findPersonByCredentials.Expect(x => x.Find(personId, tenantPassword)).Return(expected);

			var target = new WebTenantAuthentication(new FakeCurrentHttpContext(httpcontext), findPersonByCredentials, sessionDataProvider);
			target.Logon()
				.Should().Be.True();
			httpcontext.Items[WebTenantAuthenticationConfiguration.PersonInfoKey].Should().Be.SameInstanceAs(expected);
		}

		[Test]
		public void ShouldNotSetPersonInfoIfUnsuccessful_Web()
		{
			var personId = Guid.NewGuid();
			var tenantPassword = RandomName.Make();
			var httpcontext = new FakeHttpContext();

			var sessionDataProvider = MockRepository.GenerateMock<ISessionSpecificWfmCookieProvider>();
			sessionDataProvider.Expect(x => x.GrabFromCookie()).Return(new SessionSpecificData(Guid.NewGuid(), RandomName.Make(), Guid.NewGuid(), RandomName.Make(), false));
			var findPersonByCredentials = MockRepository.GenerateMock<IFindPersonInfoByCredentials>();
			findPersonByCredentials.Expect(x => x.Find(personId, tenantPassword)).Return(null);

			var target = new WebTenantAuthentication(new FakeCurrentHttpContext(httpcontext), findPersonByCredentials, sessionDataProvider);
			target.Logon()
				.Should().Be.False();
			httpcontext.Items[WebTenantAuthenticationConfiguration.PersonInfoKey].Should().Be.Null();
		}

		[Test]
		public void ShouldSetPersonInfoIfSuccessful_ExternalCall()
		{
			var personId = Guid.NewGuid();
			var tenantPassword = RandomName.Make();
			var expected = new PersonInfo();
			var httpcontext = createHttpContextWithHeadersSet(personId, tenantPassword);

			var findPersonByCredentials = MockRepository.GenerateMock<IFindPersonInfoByCredentials>();
			findPersonByCredentials.Expect(x => x.Find(personId, tenantPassword)).Return(expected);

			var target = new WebTenantAuthentication(new FakeCurrentHttpContext(httpcontext), findPersonByCredentials, MockRepository.GenerateMock<ISessionSpecificWfmCookieProvider>());
			target.Logon()
				.Should().Be.True();
			httpcontext.Items[WebTenantAuthenticationConfiguration.PersonInfoKey].Should().Be.SameInstanceAs(expected);
		}

		[Test]
		public void ShouldNotSetPersonInfoIfSuccessful_ExternalCall()
		{
			var personId = Guid.NewGuid();
			var tenantPassword = RandomName.Make();
			var httpcontext = createHttpContextWithHeadersSet(personId, tenantPassword);

			var findPersonByCredentials = MockRepository.GenerateMock<IFindPersonInfoByCredentials>();
			findPersonByCredentials.Expect(x => x.Find(personId, tenantPassword)).Return(null);

			var target = new WebTenantAuthentication(new FakeCurrentHttpContext(httpcontext), findPersonByCredentials, MockRepository.GenerateMock<ISessionSpecificWfmCookieProvider>());
			target.Logon()
				.Should().Be.False();
			httpcontext.Items[WebTenantAuthenticationConfiguration.PersonInfoKey].Should().Be.Null();
		}

		private static HttpContextBase createHttpContextWithHeadersSet(Guid personId, string tenantPassword)
		{
			var headers = new NameValueCollection {{"personid", personId.ToString()}, {"tenantpassword", tenantPassword}};
			return new FakeHttpContext(string.Empty, string.Empty, null, null, null, null, null, headers);
		}
	}
}