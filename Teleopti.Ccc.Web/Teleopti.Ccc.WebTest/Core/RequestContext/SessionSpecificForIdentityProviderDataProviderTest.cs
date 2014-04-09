using System;
using System.Web;
using log4net;
using NUnit.Framework;
using Rhino.Mocks;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Common.Time;
using Teleopti.Ccc.Web.Core.RequestContext.Cookie;
using Teleopti.Ccc.Web.Filters;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WebTest.Core.RequestContext
{
	[TestFixture]
	public class SessionSpecificForIdentityProviderDataProviderTest
	{
		private HttpResponseBase httpResponse;
		private HttpRequestBase httpRequest;
		private HttpContextBase httpContext;
		private INow now;
		private SessionSpecificForIdentityProviderDataProvider target;
		private ISessionSpecificCookieForIdentityProviderDataProviderSettings _sessionSpecificCookieForIdentityProviderDataProviderSettings;
		private HttpCookieCollection _cookieCollection;

		private static SessionSpecificData generateSessionSpecificData()
		{
			return new SessionSpecificData(Guid.NewGuid(), "DataSourceName", Guid.NewGuid());
		}

		[SetUp]
		public void Setup()
		{
			_cookieCollection = new HttpCookieCollection();
			httpResponse = MockRepository.GenerateStub<HttpResponseBase>();
			httpResponse.Stub(x => x.Cookies).Return(_cookieCollection);

			httpRequest = MockRepository.GenerateStub<HttpRequestBase>();
			httpRequest.Stub(x => x.Cookies).Return(_cookieCollection);

			httpContext = MockRepository.GenerateStub<HttpContextBase>();
			httpContext.Stub(x => x.Response).Return(httpResponse);
			httpContext.Stub(x => x.Request).Return(httpRequest);

			now = new ThisIsNow(new DateTime(2013, 9, 23, 12, 0, 0));

			_sessionSpecificCookieForIdentityProviderDataProviderSettings = new DefaultSessionSpecificCookieForIdentityProviderDataProviderSettings();
			target = new SessionSpecificForIdentityProviderDataProvider(new FakeCurrentHttpContext(httpContext), _sessionSpecificCookieForIdentityProviderDataProviderSettings, now, new SessionSpecificDataStringSerializer(MockRepository.GenerateStub<ILog>()), MockRepository.GenerateMock<ISessionAuthenticationModule>());
		}

		[Test]
		public void StoreShouldStoreSessionSpecificDataInFormsCookie()
		{
			SessionSpecificData sessionSpecificData = generateSessionSpecificData();

			target.StoreInCookie(sessionSpecificData);

			_cookieCollection[_sessionSpecificCookieForIdentityProviderDataProviderSettings.AuthenticationCookieName].Should().Not.Be.Null();
		}

		[Test]
		public void GrabShouldReturnNullWhenCookieIsMissing()
		{
			var result = target.GrabFromCookie();

			result.Should().Be.Null();
		}

		[Test]
		public void ShouldRetrieveSessionSpecificDataIfFormCookieIsPresent()
		{
			// Good enought?
			SessionSpecificData sessionSpecificData = generateSessionSpecificData();
			target.StoreInCookie(sessionSpecificData);

			var result = target.GrabFromCookie();

			result.Should().Not.Be.Null();
			result.PersonId.Should().Be.EqualTo(sessionSpecificData.PersonId);
			result.BusinessUnitId.Should().Be.EqualTo(sessionSpecificData.BusinessUnitId);
			result.DataSourceName.Should().Be.EqualTo(sessionSpecificData.DataSourceName);
		}

		[Test]
		public void ShouldExpireCookie()
		{
			SessionSpecificData sessionSpecificData = generateSessionSpecificData();
			target.StoreInCookie(sessionSpecificData);

			target.GrabFromCookie().Should().Not.Be.Null();

			target.ExpireTicket();

			target.GrabFromCookie().Should().Be.Null();
		}
	}
}