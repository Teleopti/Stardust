using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using MbCache.Core;
using Microsoft.IdentityModel.Claims;
using Microsoft.IdentityModel.Protocols.WSFederation;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.FeatureFlags;
using Teleopti.Ccc.Rta.Server;
using Teleopti.Ccc.Rta.Server.Adherence;
using Teleopti.Ccc.Web.Areas.MyTime.Models.MessageBroker;
using Teleopti.Ccc.Web.Areas.Start.Core.Authentication.DataProvider;
using Teleopti.Ccc.Web.Areas.Start.Core.Authentication.Services;
using Teleopti.Ccc.Web.Areas.Start.Models.Test;
using Teleopti.Ccc.Web.Core;
using Teleopti.Ccc.Web.Core.RequestContext;
using Teleopti.Ccc.Web.Core.RequestContext.Cookie;

namespace Teleopti.Ccc.Web.Areas.Start.Controllers
{
    public class TestController : Controller
	{
		private readonly IMutateNow _mutateNow;
	    private readonly IMbCacheFactory _cacheFactory;
	    private readonly ISessionSpecificDataProvider _sessionSpecificDataProvider;
		private readonly IAuthenticator _authenticator;
		private readonly IWebLogOn _logon;
		private readonly IBusinessUnitProvider _businessUnitProvider;
		private readonly ICurrentHttpContext _httpContext;
		private readonly IFormsAuthentication _formsAuthentication;
		private readonly IToggleManager _toggleManager;
        private readonly IIdentityProviderProvider _identityProviderProvider;

        public TestController(IMutateNow mutateNow, IMbCacheFactory cacheFactory, ISessionSpecificDataProvider sessionSpecificDataProvider, IAuthenticator authenticator, IWebLogOn logon, IBusinessUnitProvider businessUnitProvider, ICurrentHttpContext httpContext, IFormsAuthentication formsAuthentication, IToggleManager toggleManager, IIdentityProviderProvider identityProviderProvider)
		{
			_mutateNow = mutateNow;
	        _cacheFactory = cacheFactory;
	        _sessionSpecificDataProvider = sessionSpecificDataProvider;
			_authenticator = authenticator;
			_logon = logon;
			_businessUnitProvider = businessUnitProvider;
			_httpContext = httpContext;
			_formsAuthentication = formsAuthentication;
			_toggleManager = toggleManager;
		    _identityProviderProvider = identityProviderProvider;
		}

        public ViewResult BeforeScenario(bool enableMyTimeMessageBroker, string defaultProvider = null)
        {
	        if (_cacheFactory != null)
			{
				_cacheFactory.Invalidate<IDatabaseReader>();
				_cacheFactory.Invalidate<IPersonOrganizationProvider>();
	        }

			_sessionSpecificDataProvider.RemoveCookie();
			_formsAuthentication.SignOut();

			updateIocNow(null);
            ((IdentityProviderProvider)_identityProviderProvider).SetDefaultProvider(defaultProvider);
            UserDataFactory.EnableMyTimeMessageBroker = enableMyTimeMessageBroker;
			var viewModel = new TestMessageViewModel
			{
				Title = "Setting up for scenario",
				Message = "Setting up for scenario",
				ListItems = new[]
				{
					"Restoring Ccc7 database",
					"Clearing Analytics database",
					"Removing browser cookie",
					"Setting default implementation for INow"
				}
			};
			return View("Message", viewModel);
		}

		public ViewResult Logon(string dataSourceName, string businessUnitName, string userName, string password)
		{
			var result = _authenticator.AuthenticateApplicationUser(dataSourceName, userName, password);
			var businessUnits = _businessUnitProvider.RetrieveBusinessUnitsForPerson(result.DataSource, result.Person);
			var businessUnit = (from b in businessUnits where b.Name == businessUnitName select b).Single();
			
			if (result.Successful)
			{
				_formsAuthentication.SetAuthCookie(userName + "@@" + dataSourceName);
			}

			var claims = new List<Claim>
			{
				new Claim(System.IdentityModel.Claims.ClaimTypes.NameIdentifier, userName + "@@" + dataSourceName)
			};
			var claimsIdentity = new ClaimsIdentity(claims, "IssuerForTest");
			_httpContext.Current().User = new ClaimsPrincipal(new IClaimsIdentity[] { claimsIdentity });
			_logon.LogOn(dataSourceName, businessUnit.Id.Value, result.Person.Id.Value);
            
			var viewModel = new TestMessageViewModel
			                	{
			                		Title = "Quick logon",
			                		Message = "Signed in as " + result.Person.Name
			                	};
			return View("Message", viewModel);
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
			var viewModel = new TestMessageViewModel
			                	{
			                		Title = "Corrup my cookie",
			                		Message = "Cookie has been corrupted on your command!"
			                	};
			return View("Message", viewModel);
		}

		public ViewResult NonExistingDatasourceCookie()
		{
			var data = new SessionSpecificData(Guid.NewGuid(), "datasource", Guid.NewGuid());
			_sessionSpecificDataProvider.StoreInCookie(data);
			var viewModel = new TestMessageViewModel
			                	{
			                		Title = "Incorrect datasource in my cookie",
			                		Message = "Cookie has an invalid datasource on your command!"
			                	};
			return View("Message", viewModel);
		}

		public ViewResult SetCurrentTime(long ticks)
		{
			var dateSet = new DateTime(ticks);
			updateIocNow(dateSet);

			var viewModel = new TestMessageViewModel
			{
				Title = "Time changed on server!",
				Message = "Time is set to " + dateSet + " in UTC"
			};
			ViewBag.SetTime = "hello";

			return View("Message", viewModel);
		}

		public ViewResult CheckFeature(string featureName)
		{
			var result = false;
			Toggles featureToggle;

			if (Enum.TryParse(featureName, out featureToggle))
			{
				result = _toggleManager.IsEnabled(featureToggle);
			}

			var viewModel = new TestMessageViewModel
			{
				Title = string.Format("Feature {0}", featureName),
				Message = result.ToString()
			};

			return View("Message", viewModel);
		}

		[ValidateInput(false)]
		public ActionResult HandleReturn()
		{
			WSFederationMessage wsFederationMessage = WSFederationMessage.CreateFromNameValueCollection(WSFederationMessage.GetBaseUrl(ControllerContext.HttpContext.Request.Url), ControllerContext.HttpContext.Request.Form);
			if (wsFederationMessage.Context != null)
			{
				var wctx = HttpUtility.ParseQueryString(wsFederationMessage.Context);
				string returnUrl = wctx["ru"];
				if (!returnUrl.EndsWith("/"))
						returnUrl += "/";

				return new RedirectResult(returnUrl);
			}
			return new EmptyResult();
		}

		private void updateIocNow(DateTime? dateTimeSet)
		{
			_mutateNow.Mutate(dateTimeSet);
		}
	}
}
