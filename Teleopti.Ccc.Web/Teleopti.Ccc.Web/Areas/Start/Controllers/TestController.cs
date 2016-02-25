using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Web.Mvc;
using Hangfire;
using Microsoft.IdentityModel.Claims;
using Teleopti.Ccc.Domain.ApplicationLayer;
using Teleopti.Ccc.Domain.ApplicationLayer.Events;
using Teleopti.Ccc.Domain.ApplicationLayer.Rta.Service;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.FeatureFlags;
using Teleopti.Ccc.Domain.Helper;
using Teleopti.Ccc.Domain.MultiTenancy;
using Teleopti.Ccc.Infrastructure.Foundation;
using Teleopti.Ccc.Infrastructure.Hangfire;
using Teleopti.Ccc.Infrastructure.MultiTenancy.Server.Queries;
using Teleopti.Ccc.Infrastructure.Toggle;
using Teleopti.Ccc.Infrastructure.Web;
using Teleopti.Ccc.Web.Areas.MyTime.Models.MessageBroker;
using Teleopti.Ccc.Web.Areas.SSO.Core;
using Teleopti.Ccc.Web.Areas.Start.Core.Authentication.DataProvider;
using Teleopti.Ccc.Web.Areas.Start.Core.Authentication.Services;
using Teleopti.Ccc.Web.Areas.Start.Models.Test;
using Teleopti.Ccc.Web.Core;
using Teleopti.Ccc.Web.Core.RequestContext.Cookie;
using Teleopti.Ccc.Web.Core.Startup.InitializeApplication;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;
using ClaimTypes = System.IdentityModel.Claims.ClaimTypes;

namespace Teleopti.Ccc.Web.Areas.Start.Controllers
{
	public class TestController : Controller
	{
		private readonly IMutateNow _mutateNow;
		private readonly INow _now;
		private readonly ICacheInvalidator _cacheInvalidator;
		private readonly ISessionSpecificDataProvider _sessionSpecificDataProvider;
		private readonly ISsoAuthenticator _authenticator;
		private readonly IWebLogOn _logon;
		private readonly IBusinessUnitProvider _businessUnitProvider;
		private readonly ICurrentHttpContext _httpContext;
		private readonly IFormsAuthentication _formsAuthentication;
		private readonly IToggleManager _toggleManager;
		private readonly IIdentityProviderProvider _identityProviderProvider;
		private readonly ILoadPasswordPolicyService _loadPasswordPolicyService;
		private readonly ISettings _settings;
		private readonly IPhysicalApplicationPath _physicalApplicationPath;
		private readonly IFindPersonInfo _findPersonInfo;
		private readonly ActivityChangesChecker _activityChangesChecker;
		private readonly AllTenantRecurringEventPublisher _allTenantRecurringEventPublisher;
		private readonly HangfireUtilties _hangfire;

		public TestController(
			IMutateNow mutateNow, 
			INow now,
			ICacheInvalidator cacheInvalidator, 
			ISessionSpecificDataProvider sessionSpecificDataProvider, 
			ISsoAuthenticator authenticator, 
			IWebLogOn logon, 
			IBusinessUnitProvider businessUnitProvider, 
			ICurrentHttpContext httpContext, 
			IFormsAuthentication formsAuthentication, 
			IToggleManager toggleManager, 
			IIdentityProviderProvider identityProviderProvider, 
			ILoadPasswordPolicyService loadPasswordPolicyService, 
			ISettings settings, 
			IPhysicalApplicationPath physicalApplicationPath, 
			IFindPersonInfo findPersonInfo,
			ActivityChangesChecker activityChangesChecker,
			AllTenantRecurringEventPublisher allTenantRecurringEventPublisher,
			HangfireUtilties hangfire)
		{
			_mutateNow = mutateNow;
			_now = now;
			_cacheInvalidator = cacheInvalidator;
			_sessionSpecificDataProvider = sessionSpecificDataProvider;
			_authenticator = authenticator;
			_logon = logon;
			_businessUnitProvider = businessUnitProvider;
			_httpContext = httpContext;
			_formsAuthentication = formsAuthentication;
			_toggleManager = toggleManager;
			_identityProviderProvider = identityProviderProvider;
			_loadPasswordPolicyService = loadPasswordPolicyService;
			_settings = settings;
			_physicalApplicationPath = physicalApplicationPath;
			_findPersonInfo = findPersonInfo;
			_activityChangesChecker = activityChangesChecker;
			_allTenantRecurringEventPublisher = allTenantRecurringEventPublisher;
			_hangfire = hangfire;
		}

		public ViewResult BeforeScenario(bool enableMyTimeMessageBroker, string defaultProvider = null, bool usePasswordPolicy = false)
		{
			invalidateRtaCache();

			_sessionSpecificDataProvider.RemoveCookie();
			_formsAuthentication.SignOut();

			_mutateNow.Reset();

			((IdentityProviderProvider)_identityProviderProvider).SetDefaultProvider(defaultProvider);
			_loadPasswordPolicyService.ClearFile();
			_loadPasswordPolicyService.Path = Path.Combine(_physicalApplicationPath.Get(), usePasswordPolicy ? "." : _settings.ConfigurationFilesPath());

			UserDataFactory.EnableMyTimeMessageBroker = enableMyTimeMessageBroker;

			_hangfire.CancelQueue();
			_hangfire.WaitForQueue();

			clearAllConnectionPools();

			return View("Message", new TestMessageViewModel
			{
				Title = "Setting up for scenario",
				Message = "Setting up for scenario",
				ListItems = new[]
				{
					"Invalidating browser cookie",
					"Resetting faked time",
					"Doing some stuff around password policy",
					"Cancelling and waiting for hangfire jobs to finish",
					"Clearing all connection pools",
					"Restoring Ccc7 database",
					"Clearing Analytics database"
				}
			});
		}
		
		public ViewResult ClearConnections()
		{
			clearAllConnectionPools();

			return View("Message", new TestMessageViewModel
			{
				Title = "Clear connections",
				Message = "Clearing all connection pools"
			});
		}

		[TenantUnitOfWork]
		[NoTenantAuthentication]
		public virtual ViewResult Logon(string businessUnitName, string userName, string password, bool isPersistent = false, bool isLogonFromBrowser = true)
		{
			var result = _authenticator.AuthenticateApplicationUser(userName, password);
			var businessUnits = _businessUnitProvider.RetrieveBusinessUnitsForPerson(result.DataSource, result.Person);
			var businessUnit = businessUnits.Single(b => b.Name == businessUnitName);
			string tenantPassword=null;

			if (result.Successful)
			{
				_formsAuthentication.SetAuthCookie(userName + TokenIdentityProvider.ApplicationIdentifier, isPersistent, isLogonFromBrowser);
				tenantPassword = _findPersonInfo.GetById(result.Person.Id.Value).TenantPassword;
			}

			var claims = new List<Claim>
			{
				new Claim(ClaimTypes.NameIdentifier, userName + TokenIdentityProvider.ApplicationIdentifier)
			};
			var claimsIdentity = new ClaimsIdentity(claims, "IssuerForTest");
			_httpContext.Current().User = new ClaimsPrincipal(new IClaimsIdentity[] { claimsIdentity });
			_logon.LogOn(result.DataSource.DataSourceName, businessUnit.Id.Value, result.Person.Id.Value, tenantPassword, isPersistent, true);

			return View("Message", new TestMessageViewModel
			{
				Title = "Quick logon",
				Message = "Signed in as " + result.Person.Name
			});
		}

		public EmptyResult ExpireMyCookie()
		{
			_sessionSpecificDataProvider.ExpireTicket();
			_formsAuthentication.SignOut();
			return new EmptyResult();
		}

		public ViewResult CorruptMyCookie()
		{
			var wrong = Convert.ToBase64String(Convert.FromBase64String("Totally wrong"));
			_sessionSpecificDataProvider.MakeCookie("UserName", wrong, false, true);

			return View("Message", new TestMessageViewModel
			{
				Title = "Corrup my cookie",
				Message = "Cookie has been corrupted on your command!"
			});
		}

		public ViewResult NonExistingDatasourceCookie()
		{
			var data = new SessionSpecificData(Guid.NewGuid(), "datasource", Guid.NewGuid(), "tenantpassword");
			_sessionSpecificDataProvider.StoreInCookie(data, false, true);

			return View("Message", new TestMessageViewModel
			{
				Title = "Incorrect datasource in my cookie",
				Message = "Cookie has an invalid datasource on your command!"
			});
		}

		public ViewResult SetCurrentTime(long ticks, string time)
		{
			invalidateRtaCache();

			if (time != null)
				_mutateNow.Is(time.Utc());
			else
				_mutateNow.Is(new DateTime(ticks));

			_activityChangesChecker.ExecuteForTest();
			_allTenantRecurringEventPublisher.PublishHourly(new TenantHearbeatEvent());
			_hangfire.TriggerAllRecurringJobs();

			return View("Message", new TestMessageViewModel
			{
				Title = "Time changed on server!",
				Message = "Time is set to " + _now.UtcDateTime() + " in UTC"
			});
		}
		
		private static void clearAllConnectionPools()
		{
			SqlConnection.ClearAllPools();
		}

		private void invalidateRtaCache()
		{
			if (_cacheInvalidator != null)
				_cacheInvalidator.InvalidateAll();
		}
		
	}
}
