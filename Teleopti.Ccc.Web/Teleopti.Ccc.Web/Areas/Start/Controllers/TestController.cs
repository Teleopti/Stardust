using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Threading;
using System.Web.Mvc;
using Hangfire;
using Hangfire.States;
using Microsoft.IdentityModel.Claims;
using Teleopti.Ccc.Domain.ApplicationLayer.Rta.Service;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.FeatureFlags;
using Teleopti.Ccc.Domain.MultiTenancy;
using Teleopti.Ccc.Infrastructure.MultiTenancy.Server.Queries;
using Teleopti.Ccc.Infrastructure.Toggle;
using Teleopti.Ccc.Infrastructure.Web;
using Teleopti.Ccc.Web.Areas.MyTime.Models.MessageBroker;
using Teleopti.Ccc.Web.Areas.SSO.Core;
using Teleopti.Ccc.Web.Areas.Start.Core.Authentication.DataProvider;
using Teleopti.Ccc.Web.Areas.Start.Core.Authentication.Services;
using Teleopti.Ccc.Web.Areas.Start.Models.Test;
using Teleopti.Ccc.Web.Core;
using Teleopti.Ccc.Web.Core.Hangfire;
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
			ActivityChangesChecker activityChangesChecker)
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

			cancelHangfireQueue();
			waitForHangfireQueue();

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

		public void WaitForHangfireQueue()
		{
			waitForHangfireQueue();
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
		public virtual ViewResult Logon(string businessUnitName, string userName, string password)
		{
			var result = _authenticator.AuthenticateApplicationUser(userName, password);
			var businessUnits = _businessUnitProvider.RetrieveBusinessUnitsForPerson(result.DataSource, result.Person);
			var businessUnit = businessUnits.Single(b => b.Name == businessUnitName);
			string tenantPassword=null;

			if (result.Successful)
			{
				_formsAuthentication.SetAuthCookie(userName + TokenIdentityProvider.ApplicationIdentifier);
				tenantPassword = _findPersonInfo.GetById(result.Person.Id.Value).TenantPassword;
			}

			var claims = new List<Claim>
			{
				new Claim(ClaimTypes.NameIdentifier, userName + TokenIdentityProvider.ApplicationIdentifier)
			};
			var claimsIdentity = new ClaimsIdentity(claims, "IssuerForTest");
			_httpContext.Current().User = new ClaimsPrincipal(new IClaimsIdentity[] { claimsIdentity });
			_logon.LogOn(result.DataSource.DataSourceName, businessUnit.Id.Value, result.Person.Id.Value, tenantPassword);

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
			_sessionSpecificDataProvider.MakeCookie("UserName", wrong);

			return View("Message", new TestMessageViewModel
			{
				Title = "Corrup my cookie",
				Message = "Cookie has been corrupted on your command!"
			});
		}

		public ViewResult NonExistingDatasourceCookie()
		{
			var data = new SessionSpecificData(Guid.NewGuid(), "datasource", Guid.NewGuid(), "tenantpassword");
			_sessionSpecificDataProvider.StoreInCookie(data);

			return View("Message", new TestMessageViewModel
			{
				Title = "Incorrect datasource in my cookie",
				Message = "Cookie has an invalid datasource on your command!"
			});
		}

		public ViewResult SetCurrentTime(long ticks)
		{
			invalidateRtaCache();

			_mutateNow.Is(new DateTime(ticks));

			_activityChangesChecker.WaitForOneExecution();

			return View("Message", new TestMessageViewModel
			{
				Title = "Time changed on server!",
				Message = "Time is set to " + _now.UtcDateTime() + " in UTC"
			});
		}

		public ViewResult CheckFeature(string featureName)
		{
			var result = false;
			Toggles featureToggle;

			if (Enum.TryParse(featureName, out featureToggle))
			{
				result = _toggleManager.IsEnabled(featureToggle);
			}

			return View("Message", new TestMessageViewModel
			{
				Title = string.Format("Feature {0}", featureName),
				Message = result.ToString()
			});
		}

		private static void clearAllConnectionPools()
		{
			SqlConnection.ClearAllPools();
		}

		private static void cancelHangfireQueue()
		{
			var monitoring = JobStorage.Current.GetMonitoringApi();
			var jobs = monitoring.EnqueuedJobs(EnqueuedState.DefaultQueue, 0, 500);
			jobs.ForEach(j => BackgroundJob.Delete(j.Key));
		}

		private static void waitForHangfireQueue()
		{
			var monitoring = JobStorage.Current.GetMonitoringApi();
			while (true)
			{
				if (monitoring.EnqueuedCount(EnqueuedState.DefaultQueue) == 0 &&
					monitoring.FetchedCount(EnqueuedState.DefaultQueue) == 0)
				{
					break;
				}
				Thread.Sleep(20);
			}
		}
		
		private void invalidateRtaCache()
		{
			if (_cacheInvalidator != null)
				_cacheInvalidator.InvalidateAll();
		}
		
	}
}
