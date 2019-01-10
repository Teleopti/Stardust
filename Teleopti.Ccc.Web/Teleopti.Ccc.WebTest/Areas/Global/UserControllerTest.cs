using NUnit.Framework;
using Rhino.Mocks;
using System;
using System.Web;
using log4net;
using Teleopti.Ccc.Domain.Common.Time;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Ccc.Infrastructure.MultiTenancy.Server;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.TestCommon.Web;
using Teleopti.Ccc.Web.Areas.Global;
using Teleopti.Ccc.Web.Areas.Start.Core.Authentication.Services;
using Teleopti.Ccc.Web.Core;
using Teleopti.Ccc.Web.Core.RequestContext.Cookie;
using Teleopti.Ccc.WebTest.Core.RequestContext;

namespace Teleopti.Ccc.WebTest.Areas.Global
{
	public class UserControllerTest
	{
		private SessionSpecificWfmCookieProviderForTest session;

		[SetUp]
		public void Setup()
		{
			ISessionSpecificCookieSettings _sessionSpecificCookieSettingsForWfm;
			HttpCookieCollection _cookieCollection;
			string _datasourcename = "DataSourceName";
			Tenant _tenant;
			DateTime _utcTheTime = new DateTime(2013, 9, 23, 12, 0, 0);

			_cookieCollection = new HttpCookieCollection();
			var httpResponse = MockRepository.GenerateStub<HttpResponseBase>();
			httpResponse.Stub(x => x.Cookies).Return(_cookieCollection);

			var httpRequest = MockRepository.GenerateStub<HttpRequestBase>();
			httpRequest.Stub(x => x.Cookies).Return(_cookieCollection);

			var httpContext = MockRepository.GenerateStub<HttpContextBase>();
			httpContext.Stub(x => x.Response).Return(httpResponse);
			httpContext.Stub(x => x.Request).Return(httpRequest);

			var now = new MutableNow(_utcTheTime);

			var sessionSpecificCookieSettingsProvider = new SessionSpecificCookieSettingsProvider();
			_sessionSpecificCookieSettingsForWfm = sessionSpecificCookieSettingsProvider.ForWfm();
			var tenantByNameWithEnsuredTransaction = new FindTenantByNameWithEnsuredTransactionFake();
			_tenant = new Tenant(_datasourcename);
			tenantByNameWithEnsuredTransaction.Has(_tenant);
			session = new SessionSpecificWfmCookieProviderForTest(new FakeCurrentHttpContext(httpContext), sessionSpecificCookieSettingsProvider, now, new SessionSpecificDataStringSerializer(MockRepository.GenerateStub<ILog>()), new MaximumSessionTimeProvider(tenantByNameWithEnsuredTransaction));
		}

		[Test]
		public void ShouldGetTheCurrentIdentityName()
		{
			var person = PersonFactory.CreatePerson();
			var principal = new TeleoptiPrincipal(new TeleoptiIdentity("Pelle", null, null, null, null), person);
			var currentPrinciple = new FakeCurrentTeleoptiPrincipal(principal);
			var target = new UserController(currentPrinciple, new FakeIanaTimeZoneProvider(), session);
			dynamic result = target.CurrentUser();
			Assert.AreEqual("Pelle", result.UserName);
		}

		[Test]
		public void ShouldGetTheCurrentLoggonUserTimezone()
		{
			var person = PersonFactory.CreatePerson();
			person.PermissionInformation.SetDefaultTimeZone(TimeZoneInfo.Local);
			var principal = new TeleoptiPrincipal(new TeleoptiIdentity("Pelle", null, null, null, null), person);
			var currentPrinciple = new FakeCurrentTeleoptiPrincipal(principal);
			var target = new UserController(currentPrinciple, new FakeIanaTimeZoneProvider(), session);
			dynamic result = target.CurrentUser();
			Assert.AreEqual(TimeZoneInfo.Local.DisplayName, result.DefaultTimeZoneName);
		}


		[Test]
		public void ShouldGetTheCurrentLoggonUserCulture()
		{
			var person = PersonFactory.CreatePerson();
			person.PermissionInformation.SetDefaultTimeZone(TimeZoneInfo.Local);
			var culture = CultureInfoFactory.CreateUsCulture();
			var uiCulture = CultureInfoFactory.CreateSwedishCulture();
			person.PermissionInformation.SetCulture(culture);
			person.PermissionInformation.SetUICulture(uiCulture);
			var principal = new TeleoptiPrincipal(new TeleoptiIdentity("Pelle", null, null, null, null), person);
			var currentPrinciple = new FakeCurrentTeleoptiPrincipal(principal);
			var target = new UserController(currentPrinciple, new FakeIanaTimeZoneProvider(), session);
			dynamic result = target.CurrentUser();
			Assert.AreEqual(uiCulture.IetfLanguageTag, result.Language);
			Assert.AreEqual(culture.Name, result.DateFormatLocale);
		}

		[Test]
		public void ShouldGetFirstDayOfWeekForTheCurrentLoggonUserCulture()
		{
			var person = PersonFactory.CreatePerson();
			person.PermissionInformation.SetDefaultTimeZone(TimeZoneInfo.Local);
			var culture = CultureInfoFactory.CreateChineseCulture();
			person.PermissionInformation.SetCulture(culture);
			var principal = new TeleoptiPrincipal(new TeleoptiIdentity("Pelle", null, null, null, null), person);
			var currentPrinciple = new FakeCurrentTeleoptiPrincipal(principal);
			var target = new UserController(currentPrinciple, new FakeIanaTimeZoneProvider(), session);
			dynamic result = target.CurrentUser();
			Assert.AreEqual(result.FirstDayOfWeek, 1);
		}


		[Test]
		public void ShouldGetDayNamesForTheCurrentLoggonUserCulture()
		{
			var person = PersonFactory.CreatePerson();
			var culture = CultureInfoFactory.CreateChineseCulture();
			person.PermissionInformation.SetCulture(culture);
			var principal = new TeleoptiPrincipal(new TeleoptiIdentity("Pelle", null, null, null, null), person);
			var currentPrinciple = new FakeCurrentTeleoptiPrincipal(principal);
			var target = new UserController(currentPrinciple, new FakeIanaTimeZoneProvider(), session);
			dynamic result = target.CurrentUser();
			Assert.AreEqual(result.DayNames, culture.DateTimeFormat.DayNames);
		}

		[Test]
		public void ShouldGetDateTimeFormatForTheCurrentLoggonUserCulture()
		{
			var person = PersonFactory.CreatePerson();
			person.PermissionInformation.SetDefaultTimeZone(TimeZoneInfo.Local);
			var culture = CultureInfoFactory.CreateChineseCulture();
			person.PermissionInformation.SetCulture(culture);
			var principal = new TeleoptiPrincipal(new TeleoptiIdentity("Pelle", null, null, null, null), person);
			var currentPrinciple = new FakeCurrentTeleoptiPrincipal(principal);
			var target = new UserController(currentPrinciple, new FakeIanaTimeZoneProvider(), session);
			dynamic result = target.CurrentUser();
			Assert.AreEqual(result.DateTimeFormat.ShortTimePattern, culture.DateTimeFormat.ShortTimePattern);
			Assert.AreEqual(result.DateTimeFormat.AMDesignator, culture.DateTimeFormat.AMDesignator);
			Assert.AreEqual(result.DateTimeFormat.PMDesignator, culture.DateTimeFormat.PMDesignator);
		}
	}

	public class FakeIanaTimeZoneProvider : IIanaTimeZoneProvider
	{
		public string IanaToWindows(string ianaZoneId)
		{
			return ianaZoneId;
		}

		public string WindowsToIana(string windowsZoneId)
		{
			return windowsZoneId;
		}
	}
}