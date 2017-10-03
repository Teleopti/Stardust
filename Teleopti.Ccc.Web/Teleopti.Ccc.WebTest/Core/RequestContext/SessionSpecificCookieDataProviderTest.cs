﻿using System;
using System.Web;
using System.Web.Security;
using NUnit.Framework;
using Rhino.Mocks;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Common.Time;
using Teleopti.Ccc.TestCommon.Web;
using Teleopti.Ccc.Web.Core.RequestContext.Cookie;
using log4net;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.TestCommon.TestData;

namespace Teleopti.Ccc.WebTest.Core.RequestContext
{
	[TestFixture]
	public class SessionSpecificCookieDataProviderTest
	{
		private HttpResponseBase httpResponse;
		private HttpRequestBase httpRequest;
		private HttpContextBase httpContext;
		private INow now;
		private SessionSpecificWfmCookieProvider target;
		private ISessionSpecificCookieSettings _sessionSpecificCookieSettingsForWfm;
		private HttpCookieCollection _cookieCollection;

		private static SessionSpecificData generateSessionSpecificData()
		{
			return new SessionSpecificData(Guid.NewGuid(), "DataSourceName",  Guid.NewGuid(), RandomName.Make());
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

			var sessionSpecificCookieSettingsProvider = new SessionSpecificCookieSettingsProvider();
			_sessionSpecificCookieSettingsForWfm = sessionSpecificCookieSettingsProvider.ForWfm();
			target = new SessionSpecificWfmCookieProvider(new FakeCurrentHttpContext(httpContext), sessionSpecificCookieSettingsProvider, now, new SessionSpecificDataStringSerializer(MockRepository.GenerateStub<ILog>()));
		}

		[Test]
		public void StoreShouldStoreSessionSpecificDataInFormsCookie()
		{
			SessionSpecificData sessionSpecificData = generateSessionSpecificData();

			target.StoreInCookie(sessionSpecificData, false, false);

			var httpCookie = _cookieCollection[_sessionSpecificCookieSettingsForWfm.AuthenticationCookieName];
			httpCookie.Should().Not.Be.Null();
		}

		[Test]
		public void StoreTicketForShortTime()
		{
			SessionSpecificData sessionSpecificData = generateSessionSpecificData();

			target.StoreInCookie(sessionSpecificData, false, false);
			FormsAuthentication.Decrypt(
				_cookieCollection[_sessionSpecificCookieSettingsForWfm.AuthenticationCookieName].Value).Expiration.Subtract(
					now.ServerDateTime_DontUse())
				.Should()
				.Be.EqualTo(_sessionSpecificCookieSettingsForWfm.AuthenticationCookieExpirationTimeSpan);
		}

		[Test]
		public void StoreTicketForLongTime()
		{
			SessionSpecificData sessionSpecificData = generateSessionSpecificData();

			target.StoreInCookie(sessionSpecificData, true, false);
			FormsAuthentication.Decrypt(
				_cookieCollection[_sessionSpecificCookieSettingsForWfm.AuthenticationCookieName].Value).Expiration.Subtract(
					now.ServerDateTime_DontUse())
				.Should()
				.Be.EqualTo(_sessionSpecificCookieSettingsForWfm.AuthenticationCookieExpirationTimeSpanLong);
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
			target.StoreInCookie(sessionSpecificData, false, false);
	
			var result = target.GrabFromCookie();

			result.Should().Not.Be.Null();
			result.PersonId.Should().Be.EqualTo(sessionSpecificData.PersonId);
			result.BusinessUnitId.Should().Be.EqualTo(sessionSpecificData.BusinessUnitId);
			result.DataSourceName.Should().Be.EqualTo(sessionSpecificData.DataSourceName);
			result.TenantPassword.Should().Be.EqualTo(sessionSpecificData.TenantPassword);
		}

		[Test]
		public void ShouldExpireCookie()
		{
			SessionSpecificData sessionSpecificData = generateSessionSpecificData();
			target.StoreInCookie(sessionSpecificData, false, false);

			target.GrabFromCookie().Should().Not.Be.Null();

			target.ExpireTicket();

			target.GrabFromCookie().Should().Be.Null();
		}
	}
}
