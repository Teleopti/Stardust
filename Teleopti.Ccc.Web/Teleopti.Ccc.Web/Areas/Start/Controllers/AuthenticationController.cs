﻿using System;
using System.Linq;
using System.Web.Mvc;
using Teleopti.Ccc.Domain.Security;
using Teleopti.Ccc.UserTexts;
using Teleopti.Ccc.Web.Areas.Start.Core.Authentication.DataProvider;
using Teleopti.Ccc.Web.Areas.Start.Core.Authentication.Services;
using Teleopti.Ccc.Web.Areas.Start.Core.Authentication.ViewModelFactory;
using Teleopti.Ccc.Web.Areas.Start.Core.Shared;
using Teleopti.Ccc.Web.Areas.Start.Models.Authentication;
using Teleopti.Ccc.Web.Core;
using Teleopti.Ccc.Web.Filters;
using Teleopti.Ccc.Web.Models.Shared;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Web.Areas.Start.Controllers
{
	[AjaxJavaScriptRedirect]
	[OutputCache(NoStore = true, Duration = 0, VaryByParam = "None")]
	public class AuthenticationController : Controller
	{
		private readonly IAuthenticator _authenticator;
		private readonly IFormsAuthentication _formsAuthentication;
		private readonly ILayoutBaseViewModelFactory _layoutBaseViewModelFactory;
		private readonly IWebLogOn _logon;
		private readonly IRedirector _redirector;
		private readonly IDataSourcesProvider _dataSourceProvider;
		private readonly IAuthenticationViewModelFactory _viewModelFactory;

		public AuthenticationController(IAuthenticationViewModelFactory viewModelFactory, IAuthenticator authenticator, IWebLogOn logon, IFormsAuthentication formsAuthentication, ILayoutBaseViewModelFactory layoutBaseViewModelFactory, IRedirector redirector, IDataSourcesProvider dataSourceProvider)
		{
			_viewModelFactory = viewModelFactory;
			_logon = logon;
			_formsAuthentication = formsAuthentication;
			_layoutBaseViewModelFactory = layoutBaseViewModelFactory;
			_redirector = redirector;
			_dataSourceProvider = dataSourceProvider;
			_authenticator = authenticator;
		}

		public ActionResult Index()
		{
			return View();
		}

		public ActionResult SignIn()
		{
			ViewBag.LayoutBase = _layoutBaseViewModelFactory.CreateLayoutBaseViewModel();
			var signInViewModel = _viewModelFactory.CreateSignInViewModel();
			return View(signInViewModel);
		}

		public ActionResult SignInNew()
		{
			ViewBag.LayoutBase = _layoutBaseViewModelFactory.CreateLayoutBaseViewModel();
			return View();
		}

		public ActionResult SignOut(string returnUrl)
		{
			_formsAuthentication.SignOut();
			return _redirector.SignOutRedirect(returnUrl);
		}

		[HttpGet]
		public JsonResult LoadDataSources()
		{
			var applicatoinDataSources = _dataSourceProvider.RetrieveDatasourcesForApplication()
				.SelectOrEmpty(
					x => new DataSourceViewModel {Name = x.DataSourceName, DisplayName = x.DataSourceName, IsApplicationLogon = true});
			var windwowsDataSources = _dataSourceProvider.RetrieveDatasourcesForWindows()
				.SelectOrEmpty(
					x => new DataSourceViewModel {Name = x.DataSourceName, DisplayName = x.DataSourceName + " " + Resources.WindowsLogonWithBrackets, IsApplicationLogon = false});
			return Json(applicatoinDataSources.Union(windwowsDataSources), JsonRequestBehavior.AllowGet);
		}
		[HttpPost]
		public ActionResult Windows([Bind(Prefix = "SignIn")] SignInWindowsModel model)
		{
			var signInModel = new Lazy<object>(() => _viewModelFactory.CreateSignInWindowsViewModel(model));
			const string winView = "SignInWindowsPartial";

			if (!ModelState.IsValid)
				if (IsJsonRequest())
					return ModelState.ToJson();
				else
					return PartialView(winView, signInModel.Value);

			var authenticationResult = _authenticator.AuthenticateWindowsUser(model.DataSourceName);
			return tryLogon(winView, signInModel, authenticationResult, AuthenticationTypeOption.Windows);
		}

		[HttpPost]
		public ActionResult Application([Bind(Prefix = "SignIn")] SignInApplicationModel model)
		{
			var signInModel = new Lazy<object>(() => _viewModelFactory.CreateSignInApplicationViewModel(model));
			const string appView = "SignInApplicationPartial";

			if (!ModelState.IsValid)
				if (IsJsonRequest())
					return PrepareAndReturnJsonError(null);
				else
					return PartialView(appView, signInModel.Value);

			var authenticationResult = _authenticator.AuthenticateApplicationUser(model.DataSourceName, model.UserName,
			                                                                      model.Password);
			return tryLogon(appView, signInModel, authenticationResult, AuthenticationTypeOption.Application);
		}

		private ActionResult tryLogon(string currentView, Lazy<object> signInModel, AuthenticateResult authenticationResult, AuthenticationTypeOption authenticationType)
		{
			if (!authenticationResult.Successful)
			{
				ModelState.AddModelError(string.Empty,
				                         authenticationResult.HasMessage
				                         	? authenticationResult.Message
				                         	: Resources.LogOnFailedInvalidUserNameOrPassword);
				if (IsJsonRequest())
				{
					return PrepareAndReturnJsonError(null);
				}
				return PartialView(currentView, signInModel.Value);
			}

			var businessUnitViewModel = _viewModelFactory.CreateBusinessUnitViewModel(authenticationResult.DataSource,
			                                                                          authenticationResult.Person,
			                                                                          authenticationType, authenticationResult.Message);
			switch (businessUnitViewModel.BusinessUnits.Count())
			{
				case 0:
					ModelState.AddModelError(string.Empty, Resources.NoAllowedBusinessUnitFoundInCurrentDatabase);
					if (IsJsonRequest())
					{
						return PrepareAndReturnJsonError(null);
					}
					return PartialView(currentView, signInModel.Value);
				case 1:
					var businessUnitId = businessUnitViewModel.BusinessUnits.First().Id;
					var persionId = authenticationResult.Person.Id.Value;
					var dataSourceName = authenticationResult.DataSource.DataSourceName;
					return tryLogOnAndReturnResult(businessUnitId, dataSourceName, persionId, authenticationType, authenticationResult.Message);
				default:
					if (IsJsonRequest())
					{
						return Json(businessUnitViewModel);
					}
					return PartialView("SignInBusinessUnitPartial", businessUnitViewModel);
			}
		}

		[HttpPost]
		public ActionResult Logon([Bind(Prefix = "SignIn")] SignInBusinessUnitModel model)
		{
			return tryLogOnAndReturnResult(model.BusinessUnitId, model.DataSourceName, model.PersonId, (AuthenticationTypeOption)model.AuthenticationType, model.WarningMessage);
		}

		private ActionResult tryLogOnAndReturnResult(Guid businessUnitId, string dataSourceName, Guid personId, AuthenticationTypeOption authenticationType, string warningMessage)
		{
			try
			{
				_logon.LogOn(businessUnitId, dataSourceName, personId, authenticationType, warningMessage);

				return _redirector.SignInRedirect();
			}
			catch (PermissionException)
			{
				var errorViewModel = new ErrorViewModel { Message = Resources.InsufficientPermissionForWeb };
				if (IsJsonRequest())
				{
					return PrepareAndReturnJsonError(new JsonResult {Data = errorViewModel});
				}
				return PartialView("ErrorPartial", errorViewModel);
			}
		}

		[HttpGet]
		public JsonResult PopWarningMessage()
		{
			var message = _logon.PopWarningMessage();
			return Json(new {Message = message, HasMessage = !string.IsNullOrEmpty(message)},JsonRequestBehavior.AllowGet);
		}

		private JsonResult PrepareAndReturnJsonError(JsonResult result)
		{
			Response.TrySkipIisCustomErrors = true;
			Response.StatusCode = 400;
			return result ?? ModelState.ToJson();
		}

		private bool IsJsonRequest()
		{
			return HttpContext != null && HttpContext.Request.AcceptsJson();
		}


		public ViewResult MobileSignIn()
		{
			ViewBag.LayoutBase = "Mobile SignIn";
			var signInViewModel = _viewModelFactory.CreateSignInViewModel();
			return View(signInViewModel);
		}
	}
}