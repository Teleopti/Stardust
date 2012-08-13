using System;
using System.Web;
using System.Web.Security;
using NUnit.Framework;
using Rhino.Mocks;
using SharpTestsEx;
using Teleopti.Ccc.Web.Core.RequestContext.Cookie;
using Teleopti.Interfaces.Domain;
using log4net;

namespace Teleopti.Ccc.WebTest.Core.RequestContext
{
	[TestFixture]
	public class SessionSpecificCookieDataProviderTest
	{
		private HttpResponseBase httpResponse;
		private HttpRequestBase httpRequest;
		private HttpContextBase httpContext;
		private INow now;
		private DateTime fictiveNow;
		private SessionSpecificCookieDataProvider target;
		private ISessionSpecificCookieDataProviderSettings _sessionSpecificCookieDataProviderSettings;
		private HttpCookieCollection _cookieCollection;

		private static SessionSpecificData generateSessionSpecificData()
		{
			return new SessionSpecificData(Guid.NewGuid(), "DataSourceName",  Guid.NewGuid(), AuthenticationTypeOption.Application);
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

			// Need to remove dependecies to FormsAuthentication.It uses DataTime.Now to check for expiration - test fails with past dates
			fictiveNow = DateTime.Now; //new DateTime(2000,1,1,1,1,1);
			now = MockRepository.GenerateStub<INow>();
			now.Stub(x => x.Time).Return(fictiveNow);

			_sessionSpecificCookieDataProviderSettings = new DefaultSessionSpecificCookieDataProviderSettings();
			target = new SessionSpecificCookieDataProvider(new FakeCurrentHttpContext(httpContext), _sessionSpecificCookieDataProviderSettings, now, new SessionSpecificDataStringSerializer(MockRepository.GenerateStub<ILog>()));
		}

		[Test]
		public void StoreShouldStoreSessionSpecificDataInFormsCookie()
		{
			SessionSpecificData sessionSpecificData = generateSessionSpecificData();
			
			target.StoreInCookie(sessionSpecificData);

			_cookieCollection[FormsAuthentication.FormsCookieName].Should().Not.Be.Null();
			
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
