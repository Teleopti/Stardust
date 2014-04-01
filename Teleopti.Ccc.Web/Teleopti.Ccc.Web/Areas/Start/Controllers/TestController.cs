using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Microsoft.IdentityModel.Claims;
using Microsoft.IdentityModel.Protocols.WSFederation;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Web.Areas.MyTime.Models.MessageBroker;
using Teleopti.Ccc.Web.Areas.Start.Core.Authentication.DataProvider;
using Teleopti.Ccc.Web.Areas.Start.Core.Authentication.Services;
using Teleopti.Ccc.Web.Areas.Start.Models.Test;
using Teleopti.Ccc.Web.Core;
using Teleopti.Ccc.Web.Core.RequestContext.Cookie;

namespace Teleopti.Ccc.Web.Areas.Start.Controllers
{
	public class TestController : Controller
	{
		private readonly IMutateNow _mutateNow;
		private readonly ISessionSpecificDataProvider _sessionSpecificDataProvider;
		private readonly IAuthenticator _authenticator;
		private readonly IWebLogOn _logon;
		private readonly IBusinessUnitProvider _businessUnitProvider;

		public TestController(IMutateNow mutateNow, ISessionSpecificDataProvider sessionSpecificDataProvider, IAuthenticator authenticator, IWebLogOn logon, IBusinessUnitProvider businessUnitProvider)
		{
			_mutateNow = mutateNow;
			_sessionSpecificDataProvider = sessionSpecificDataProvider;
			_authenticator = authenticator;
			_logon = logon;
			_businessUnitProvider = businessUnitProvider;
		}

		public ViewResult BeforeScenario(bool enableMyTimeMessageBroker)
		{
			_sessionSpecificDataProvider.RemoveCookie();

			updateIocNow(null);
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
			
			var claims = new List<Claim>
			{
				new Claim(System.IdentityModel.Claims.ClaimTypes.NameIdentifier, userName + "§" + dataSourceName)
			};
			var claimsIdentity = new ClaimsIdentity(claims,"MyType");
			HttpContext.User = new ClaimsPrincipal(new IClaimsIdentity[] { claimsIdentity });

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

		[ValidateInput(false)]
		public ActionResult HandleReturn()
		{
			WSFederationMessage wsFederationMessage = WSFederationMessage.CreateFromNameValueCollection(WSFederationMessage.GetBaseUrl(ControllerContext.HttpContext.Request.Url), ControllerContext.HttpContext.Request.Form);
			if (wsFederationMessage.Context != null)
			{
				var wctx = HttpUtility.ParseQueryString(wsFederationMessage.Context);
				string returnUrl = wctx["ru"];

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
