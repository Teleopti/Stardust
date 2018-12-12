using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Security.Claims;
using System.Web.Mvc;
using Teleopti.Ccc.Domain.Aop;
using Teleopti.Ccc.Domain.ApplicationLayer;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Common.Time;
using Teleopti.Ccc.Domain.Helper;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Domain.MultiTenancy;
using Teleopti.Ccc.Infrastructure.Foundation;
using Teleopti.Ccc.Infrastructure.Hangfire;
using Teleopti.Ccc.Infrastructure.MultiTenancy.Server.Queries;
using Teleopti.Ccc.Infrastructure.Web;
using Teleopti.Ccc.Web.Areas.MyTime.Models.MessageBroker;
using Teleopti.Ccc.Web.Areas.SSO.Core;
using Teleopti.Ccc.Web.Areas.Start.Core.Authentication.DataProvider;
using Teleopti.Ccc.Web.Areas.Start.Core.Authentication.Services;
using Teleopti.Ccc.Web.Areas.Start.Models.Test;
using Teleopti.Ccc.Web.Core;
using Teleopti.Ccc.Web.Core.RequestContext.Cookie;
using Teleopti.Ccc.Web.Core.Startup.InitializeApplication;
using ClaimTypes = System.IdentityModel.Claims.ClaimTypes;

namespace Teleopti.Ccc.Web.Areas.Start.Controllers
{
	public class TestController : Controller
	{
		private readonly IMutateNow _mutateNow;
		private readonly INow _now;
		private readonly ISessionSpecificWfmCookieProvider _sessionSpecificWfmCookieProvider;
		private readonly ISsoAuthenticator _authenticator;
		private readonly IWebLogOn _logon;
		private readonly IBusinessUnitProvider _businessUnitProvider;
		private readonly ICurrentHttpContext _httpContext;
		private readonly IFormsAuthentication _formsAuthentication;
		private readonly IIdentityProviderProvider _identityProviderProvider;
		private readonly ILoadPasswordPolicyService _loadPasswordPolicyService;
		private readonly ISettings _settings;
		private readonly IPhysicalApplicationPath _physicalApplicationPath;
		private readonly IFindPersonInfo _findPersonInfo;
		private readonly HangfireUtilities _hangfire;
		private readonly RecurringEventPublishings _recurringEventPublishings;
		private readonly SystemVersion _version;

		public TestController(
			IMutateNow mutateNow,
			INow now,
			ISessionSpecificWfmCookieProvider sessionSpecificWfmCookieProvider,
			ISsoAuthenticator authenticator,
			IWebLogOn logon,
			IBusinessUnitProvider businessUnitProvider,
			ICurrentHttpContext httpContext,
			IFormsAuthentication formsAuthentication,
			IIdentityProviderProvider identityProviderProvider,
			ILoadPasswordPolicyService loadPasswordPolicyService,
			ISettings settings,
			IPhysicalApplicationPath physicalApplicationPath,
			IFindPersonInfo findPersonInfo,
			HangfireUtilities hangfire,
			RecurringEventPublishings recurringEventPublishings,
			SystemVersion version)
		{
			_mutateNow = mutateNow;
			_now = now;
			_sessionSpecificWfmCookieProvider = sessionSpecificWfmCookieProvider;
			_authenticator = authenticator;
			_logon = logon;
			_businessUnitProvider = businessUnitProvider;
			_httpContext = httpContext;
			_formsAuthentication = formsAuthentication;
			_identityProviderProvider = identityProviderProvider;
			_loadPasswordPolicyService = loadPasswordPolicyService;
			_settings = settings;
			_physicalApplicationPath = physicalApplicationPath;
			_findPersonInfo = findPersonInfo;
			_hangfire = hangfire;
			_recurringEventPublishings = recurringEventPublishings;
			_version = version;
		}

		[TestLog]
		public virtual ViewResult BeforeScenario(
			string name = "Teapots are nice!",
			bool enableMyTimeMessageBroker = false,
			string defaultProvider = "Teleopti",
			bool usePasswordPolicy = false)
		{
			_sessionSpecificWfmCookieProvider.RemoveCookie();
			_formsAuthentication.SignOut();
			_mutateNow.Reset();
			_version.Reset();

			((IdentityProviderProvider) _identityProviderProvider).SetDefaultProvider(defaultProvider);
			_loadPasswordPolicyService.ClearFile();
			_loadPasswordPolicyService.Path = Path.Combine(_physicalApplicationPath.Get(), usePasswordPolicy ? "" : _settings.ConfigurationFilesPath());

			UserDataFactory.EnableMyTimeMessageBroker = enableMyTimeMessageBroker;

			clearAllConnectionPools();

			return View("Message", new TestMessageViewModel
			{
				Title = $"Setting up for test {name}",
				Message = $"Setting up for test {name}",
				ListItems = new[]
				{
					"Invalidating browser cookie",
					"Resetting faked time",
					"Doing some stuff around password policy",
					"Cancelling and waiting for hangfire jobs to finish",
					"Clearing all connection pools",
					"Restoring Ccc7 database",
					"Restoring Analytics database"
				}
			});
		}

		[TestLog]
		public virtual ViewResult Start()
		{
			return View("Message", new TestMessageViewModel
			{
				Title = "Start application",
				Message = "Application started"
			});
		}

		[TestLog]
		public virtual ViewResult ClearConnections()
		{
			clearAllConnectionPools();

			return View("Message", new TestMessageViewModel
			{
				Title = "Clear connections",
				Message = "Clearing all connection pools"
			});
		}

		private static void clearAllConnectionPools()
		{
			SqlConnection.ClearAllPools();
		}

		[TenantUnitOfWork]
		[NoTenantAuthentication]
		[TestLog]
		public virtual ViewResult Logon(string businessUnitName, string userName, string password, bool isPersistent = false, bool isLogonFromBrowser = true)
		{
			var result = _authenticator.AuthenticateApplicationUser(userName, password);
			var businessUnits = _businessUnitProvider.RetrieveBusinessUnitsForPerson(result.DataSource, result.Person);
			var businessUnit = businessUnits.Single(b => b.Name == businessUnitName);
			string tenantPassword = null;

			if (result.Successful)
			{
				_formsAuthentication.SetAuthCookie(userName + TokenIdentityProvider.ApplicationIdentifier, isPersistent, isLogonFromBrowser, result.DataSource.DataSourceName);
				tenantPassword = _findPersonInfo.GetById(result.Person.Id.Value).TenantPassword;
			}

			var claims = new List<Claim>
			{
				new Claim(ClaimTypes.NameIdentifier, userName + TokenIdentityProvider.ApplicationIdentifier)
			};
			var claimsIdentity = new ClaimsIdentity(claims, "IssuerForTest");
			_httpContext.Current().User = new ClaimsPrincipal(new[] {claimsIdentity});
			_logon.LogOn(result.DataSource.DataSourceName, businessUnit.Id.Value, result.Person, tenantPassword, isPersistent, true);

			return View("Message", new TestMessageViewModel
			{
				Title = "Quick logon",
				Message = "Signed in as " + result.Person.Name
			});
		}

		[TestLog]
		public virtual EmptyResult ExpireMyCookie()
		{
			_sessionSpecificWfmCookieProvider.ExpireTicket();
			_formsAuthentication.SignOut();
			return new EmptyResult();
		}

		[TestLog]
		public virtual ViewResult CorruptMyCookie()
		{
			var wrong = Convert.ToBase64String(Convert.FromBase64String("Totally wrong"));
			_sessionSpecificWfmCookieProvider.MakeCookie("UserName", wrong, false, true, "");

			return View("Message", new TestMessageViewModel
			{
				Title = "Corrup my cookie",
				Message = "Cookie has been corrupted on your command!"
			});
		}

		[TestLog]
		public virtual ViewResult NonExistingDatasourceCookie()
		{
			var dataSourceName = "datasource";
			var data = new SessionSpecificData(Guid.NewGuid(), dataSourceName, Guid.NewGuid(), "tenantpassword", false);
			_sessionSpecificWfmCookieProvider.StoreInCookie(data, false, true, dataSourceName);

			return View("Message", new TestMessageViewModel
			{
				Title = "Incorrect datasource in my cookie",
				Message = "Cookie has an invalid datasource on your command!"
			});
		}

		[TestLog]
		[HttpGet]
		public virtual ViewResult SetVersion(string version)
		{
			_version.Is(version);

			return View("Message", new TestMessageViewModel
			{
				Title = "Version changed on server!",
				Message = $"Version is set to {version}"
			});
		}

		[TestLog]
		[HttpGet]
		[ActionName("TriggerRecurringJobs")]
		public virtual void TriggerRecurringJobs()
		{
			_recurringEventPublishings.WithPublishingsForTest(() =>
			{
				_hangfire.TriggerRecurringJobs();
			});
		}

		[TestLog]
		[HttpGet]
		public virtual ViewResult SetCurrentTime(long? ticks, string time)
		{
			setCurrentTime(ticks, time, true);

			return View("Message", new TestMessageViewModel
			{
				Title = "Time changed on server!",
				Message = "Time is set to " + _now.UtcDateTime() + " in UTC"
			});
		}

		[TestLog]
		[HttpPost]
		[ActionName("SetCurrentTime")]
		public virtual void SetCurrentTimePost(long? ticks, string time, bool waitForQueue = true) =>
			setCurrentTime(ticks, time, waitForQueue);

		private void setCurrentTime(long? ticks, string time, bool waitForQueue)
		{
			var oldNow = _now.UtcDateTime();
			if (ticks.HasValue)
				_mutateNow.Is(DateTime.SpecifyKind(new DateTime(ticks.Value), DateTimeKind.Utc));
			else
				_mutateNow.Is(time.Utc());
			var newNow = _now.UtcDateTime();

			_recurringEventPublishings.WithPublishingsForTest(() =>
			{
				var timePassed = new TimePassingSimulator(oldNow, newNow);
				timePassed.IfDayPassed(_hangfire.TriggerDailyRecurringJobs);
				timePassed.IfHourPassed(_hangfire.TriggerHourlyRecurringJobs);
				timePassed.IfMinutePassed(_hangfire.TriggerMinutelyRecurringJobs);
			});

			if (waitForQueue)
				_hangfire.WaitForQueue();
		}
	}
}