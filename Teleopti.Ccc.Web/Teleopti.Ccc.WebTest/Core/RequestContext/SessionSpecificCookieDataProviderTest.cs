using System;
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
using Teleopti.Ccc.Domain.MultiTenancy;
using Teleopti.Ccc.Infrastructure.MultiTenancy.Server;
using Teleopti.Ccc.Infrastructure.Web;
using Teleopti.Ccc.TestCommon.TestData;
using Teleopti.Ccc.Web.Areas.Start.Core.Authentication.Services;

namespace Teleopti.Ccc.WebTest.Core.RequestContext
{
	[TestFixture]
	public class SessionSpecificCookieDataProviderTest
	{
		private HttpResponseBase httpResponse;
		private HttpRequestBase httpRequest;
		private HttpContextBase httpContext;
		private MutableNow now;
		private SessionSpecificWfmCookieProviderForTest target;
		private ISessionSpecificCookieSettings _sessionSpecificCookieSettingsForWfm;
		private HttpCookieCollection _cookieCollection;
		private static string _datasourcename = "DataSourceName";
		private Tenant _tenant;
		private readonly DateTime _utcTheTime = new DateTime(2013, 9, 23, 12, 0, 0);

		private static SessionSpecificData generateSessionSpecificData()
		{
			return new SessionSpecificData(Guid.NewGuid(), _datasourcename,  Guid.NewGuid(), RandomName.Make(), false);
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

			now = new MutableNow(_utcTheTime);

			var sessionSpecificCookieSettingsProvider = new SessionSpecificCookieSettingsProvider();
			_sessionSpecificCookieSettingsForWfm = sessionSpecificCookieSettingsProvider.ForWfm();
			var tenantByNameWithEnsuredTransaction = new FindTenantByNameWithEnsuredTransactionFake();
			_tenant = new Tenant(_datasourcename);
			tenantByNameWithEnsuredTransaction.Has(_tenant);
			target = new SessionSpecificWfmCookieProviderForTest(new FakeCurrentHttpContext(httpContext), sessionSpecificCookieSettingsProvider, now, new SessionSpecificDataStringSerializer(MockRepository.GenerateStub<ILog>()), new MaximumSessionTimeProvider(tenantByNameWithEnsuredTransaction));
		}

		[Test]
		public void StoreShouldStoreSessionSpecificDataInFormsCookie()
		{
			SessionSpecificData sessionSpecificData = generateSessionSpecificData();

			target.StoreInCookie(sessionSpecificData, false, false, sessionSpecificData.DataSourceName);

			var httpCookie = _cookieCollection[_sessionSpecificCookieSettingsForWfm.AuthenticationCookieName];
			httpCookie.Should().Not.Be.Null();
		}

		[Test]
		public void StoreTicketForShortTime()
		{
			SessionSpecificData sessionSpecificData = generateSessionSpecificData();

			target.StoreInCookie(sessionSpecificData, false, false, sessionSpecificData.DataSourceName);
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

			target.StoreInCookie(sessionSpecificData, true, false, sessionSpecificData.DataSourceName);
			FormsAuthentication.Decrypt(
				_cookieCollection[_sessionSpecificCookieSettingsForWfm.AuthenticationCookieName].Value).Expiration.Subtract(
					now.ServerDateTime_DontUse())
				.Should()
				.Be.EqualTo(_sessionSpecificCookieSettingsForWfm.AuthenticationCookieExpirationTimeSpanLong);
		}

		[Test]
		public void StoreTicketForLongTimeUseMaximumSessionTimeInConfiguration()
		{
			_tenant.SetApplicationConfig(TenantApplicationConfigKey.MaximumSessionTimeInMinutes, "480");
			SessionSpecificData sessionSpecificData = generateSessionSpecificData();

			target.StoreInCookie(sessionSpecificData, true, false, sessionSpecificData.DataSourceName);
			FormsAuthentication.Decrypt(
				_cookieCollection[_sessionSpecificCookieSettingsForWfm.AuthenticationCookieName].Value).Expiration.Subtract(
					now.ServerDateTime_DontUse())
				.Should()
				.Be.EqualTo(TimeSpan.FromMinutes(480));
		}

		[Test]
		public void UseMaximumSessionTimeInConfigurationAsShortTimeIfMaximumSessionTimeInConfigurationLessThanDefault()
		{
			_tenant.SetApplicationConfig(TenantApplicationConfigKey.MaximumSessionTimeInMinutes, "29");
			SessionSpecificData sessionSpecificData = generateSessionSpecificData();

			target.StoreInCookie(sessionSpecificData, false, false, sessionSpecificData.DataSourceName);
			FormsAuthentication.Decrypt(
				_cookieCollection[_sessionSpecificCookieSettingsForWfm.AuthenticationCookieName].Value).Expiration.Subtract(
					now.ServerDateTime_DontUse())
				.Should()
				.Be.EqualTo(TimeSpan.FromMinutes(29));
		}

		[Test]
		public void UseDefaultAsShortTimeIfMaximumSessionTimeInConfigurationLargerThanDefault()
		{
			_tenant.SetApplicationConfig(TenantApplicationConfigKey.MaximumSessionTimeInMinutes, "31");
			SessionSpecificData sessionSpecificData = generateSessionSpecificData();

			target.StoreInCookie(sessionSpecificData, false, false, sessionSpecificData.DataSourceName);
			FormsAuthentication.Decrypt(
				_cookieCollection[_sessionSpecificCookieSettingsForWfm.AuthenticationCookieName].Value).Expiration.Subtract(
					now.ServerDateTime_DontUse())
				.Should()
				.Be.EqualTo(_sessionSpecificCookieSettingsForWfm.AuthenticationCookieExpirationTimeSpan);
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
		public void ShouldSlideExpiration()
		{
			SessionSpecificData sessionSpecificData = generateSessionSpecificData();
			target.StoreInCookie(sessionSpecificData, false, false, sessionSpecificData.DataSourceName);

			var newDateTime = _utcTheTime.AddMinutes(16);
			now.Is(newDateTime);

			var result = target.GrabFromCookie();

			target.GetTicket().IssueDate.Should().Be.EqualTo(newDateTime.ToLocalTime());

			result.Should().Not.Be.Null();
			result.PersonId.Should().Be.EqualTo(sessionSpecificData.PersonId);
			result.BusinessUnitId.Should().Be.EqualTo(sessionSpecificData.BusinessUnitId);
			result.DataSourceName.Should().Be.EqualTo(sessionSpecificData.DataSourceName);
			result.TenantPassword.Should().Be.EqualTo(sessionSpecificData.TenantPassword);
		}

		[Test]
		public void ShouldNotSlideExpirationWhenHaveMaximumSessionTimeFromConfiguration()
		{
			_tenant.SetApplicationConfig(TenantApplicationConfigKey.MaximumSessionTimeInMinutes, "480");
			SessionSpecificData sessionSpecificData = generateSessionSpecificData();
			target.StoreInCookie(sessionSpecificData, false, false, sessionSpecificData.DataSourceName);

			var newDateTime = _utcTheTime.AddMinutes(16);
			now.Is(newDateTime);

			var result = target.GrabFromCookie();

			target.GetTicket().IssueDate.Should().Be.EqualTo(_utcTheTime.ToLocalTime());

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

		[Test]
		public void ShouldReturnNullWhenCookieValueIsNotAValidValue()
		{
			SessionSpecificData sessionSpecificData = generateSessionSpecificData();
			target.StoreInCookie(sessionSpecificData, false, false, sessionSpecificData.DataSourceName);

			httpContext.Response.Cookies[_sessionSpecificCookieSettingsForWfm.AuthenticationCookieName]
				.Value += "X";

			var result = target.GrabFromCookie();

			result.Should().Be.Null();
		}

		[Test]
		public void ShouldReturnNullWhenCookieValueIsNotAValidBase64String()
		{
			SessionSpecificData sessionSpecificData = generateSessionSpecificData();
			target.StoreInCookie(sessionSpecificData, false, false, sessionSpecificData.DataSourceName);

			var cookieValue = httpContext.Response.Cookies[_sessionSpecificCookieSettingsForWfm.AuthenticationCookieName]
				.Value;

			var invalidCookieValue = cookieValue.Substring(0, cookieValue.Length - 2) + "=" + cookieValue.Substring(cookieValue.Length - 1);
			httpContext.Response.Cookies[_sessionSpecificCookieSettingsForWfm.AuthenticationCookieName]
				.Value = invalidCookieValue;

			var result = target.GrabFromCookie();

			result.Should().Be.Null();
		}
	}

	internal class SessionSpecificWfmCookieProviderForTest : SessionSpecificWfmCookieProvider
	{
		public SessionSpecificWfmCookieProviderForTest(ICurrentHttpContext httpContext, SessionSpecificCookieSettingsProvider sessionSpecificCookieSettingsProvider, INow now, ISessionSpecificDataStringSerializer dataStringSerializer, MaximumSessionTimeProvider maximumSessionTimeProvider) : base(httpContext, sessionSpecificCookieSettingsProvider, now, dataStringSerializer, maximumSessionTimeProvider)
		{
		}

		public FormsAuthenticationTicket GetTicket()
		{
			var cookie = GetCookie();
			return DecryptCookie(cookie);
		}
	}
}
