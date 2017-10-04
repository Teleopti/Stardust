using System;
using System.Web;
using log4net;
using NUnit.Framework;
using Rhino.Mocks;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Common.Time;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Infrastructure.MultiTenancy.Server;
using Teleopti.Ccc.TestCommon.TestData;
using Teleopti.Ccc.TestCommon.Web;
using Teleopti.Ccc.Web.Areas.Start.Core.Authentication.Services;
using Teleopti.Ccc.Web.Core.RequestContext.Cookie;

namespace Teleopti.Ccc.WebTest.Core.RequestContext
{
	[TestFixture]
	public class SessionSpecificForIdentityProviderDataProviderTest
	{
		private HttpResponseBase httpResponse;
		private HttpRequestBase httpRequest;
		private HttpContextBase httpContext;
		private INow now;
		private SessionSpecificTeleoptiCookieProvider target;
		private ISessionSpecificCookieSettings _sessionSpecificCookieSettingsForTeleoptiIdentityProvider;
		private HttpCookieCollection _cookieCollection;
		private FindTenantByNameWithEnsuredTransactionFake _findTenantByNameWithEnsuredTransactionFake;
		private static string _datasourcename = "DataSourceName";


		private static SessionSpecificData generateSessionSpecificData()
		{
			return new SessionSpecificData(Guid.NewGuid(), _datasourcename, Guid.NewGuid(), RandomName.Make());
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
			_findTenantByNameWithEnsuredTransactionFake = new FindTenantByNameWithEnsuredTransactionFake();
			_findTenantByNameWithEnsuredTransactionFake.Has(new Tenant(_datasourcename));
			_sessionSpecificCookieSettingsForTeleoptiIdentityProvider = sessionSpecificCookieSettingsProvider.ForTeleopti();
			target = new SessionSpecificTeleoptiCookieProvider(new FakeCurrentHttpContext(httpContext), sessionSpecificCookieSettingsProvider, now, new SessionSpecificDataStringSerializer(MockRepository.GenerateStub<ILog>()), new MaximumSessionTimeProvider(_findTenantByNameWithEnsuredTransactionFake));
		}

		[Test]
		public void StoreShouldStoreSessionSpecificDataInFormsCookie()
		{
			SessionSpecificData sessionSpecificData = generateSessionSpecificData();

			target.StoreInCookie(sessionSpecificData, false, false, sessionSpecificData.DataSourceName);

			_cookieCollection[_sessionSpecificCookieSettingsForTeleoptiIdentityProvider.AuthenticationCookieName].Should().Not.Be.Null();
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
			target.StoreInCookie(sessionSpecificData, false, false, sessionSpecificData.DataSourceName);

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
			target.StoreInCookie(sessionSpecificData, false, false, sessionSpecificData.DataSourceName);

			target.GrabFromCookie().Should().Not.Be.Null();

			target.ExpireTicket();

			target.GrabFromCookie().Should().Be.Null();
		}
	}
}