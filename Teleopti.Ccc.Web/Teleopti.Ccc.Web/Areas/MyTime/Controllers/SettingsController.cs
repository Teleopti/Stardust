﻿using System.Globalization;
using System.Web;
using System.Web.Mvc;
using Teleopti.Ccc.Domain.Security.AuthorizationData;
using Teleopti.Ccc.Domain.SystemSetting.GlobalSetting;
using Teleopti.Ccc.Infrastructure.MultiTenancy.Server.NHibernate;
using Teleopti.Ccc.Web.Areas.MultiTenancy.Core;
using Teleopti.Ccc.Web.Areas.MyTime.Core;
using Teleopti.Ccc.Web.Areas.MyTime.Core.Filters;
using Teleopti.Ccc.Web.Areas.MyTime.Core.Settings.DataProvider;
using Teleopti.Ccc.Web.Areas.MyTime.Core.Settings.ViewModelFactory;
using Teleopti.Ccc.Web.Areas.MyTime.Models.Settings;
using Teleopti.Ccc.Web.Filters;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Web.Areas.MyTime.Controllers
{
	public class SettingsController : Controller
	{
		private readonly ILoggedOnUser _loggedOnUser;
		private readonly IPersonPersister _personPersister;
		private readonly ISettingsPermissionViewModelFactory _settingsPermissionViewModelFactory;
		private readonly ISettingsPersisterAndProvider<CalendarLinkSettings> _calendarLinkSettingsPersisterAndProvider;
		private readonly ICalendarLinkViewModelFactory _calendarLinkViewModelFactory;
		private readonly IChangePersonPassword _changePersonPassword;
		private readonly ISettingsViewModelFactory _settingsViewModelFactory;
		private readonly ISettingsPersisterAndProvider<NameFormatSettings>_nameFormatSettingsPersisterAndProvider;

		public SettingsController(ILoggedOnUser loggedOnUser,
		                          IPersonPersister personPersister,
		                          ISettingsPermissionViewModelFactory settingsPermissionViewModelFactory,
										  ISettingsViewModelFactory settingsViewModelFactory,
										  ISettingsPersisterAndProvider<CalendarLinkSettings> calendarLinkSettingsPersisterAndProvider,
											ISettingsPersisterAndProvider<NameFormatSettings> nameFormatSettingsPersisterAndProvider,
											ICalendarLinkViewModelFactory calendarLinkViewModelFactory,
											IChangePersonPassword changePersonPassword)
		{
			_loggedOnUser = loggedOnUser;
			_personPersister = personPersister;
			_settingsPermissionViewModelFactory = settingsPermissionViewModelFactory;
			_settingsViewModelFactory = settingsViewModelFactory;
			_calendarLinkSettingsPersisterAndProvider = calendarLinkSettingsPersisterAndProvider;
			_nameFormatSettingsPersisterAndProvider = nameFormatSettingsPersisterAndProvider;
			_calendarLinkViewModelFactory = calendarLinkViewModelFactory;
			_changePersonPassword = changePersonPassword;
		}

		[EnsureInPortal]
		[UnitOfWorkAction]
		[HttpGet]
		public ViewResult Index()
		{
			return View("RegionalSettingsPartial", _settingsPermissionViewModelFactory.CreateViewModel());
		}

		[EnsureInPortal]
		[HttpGet]
		public ViewResult Password()
		{
			return View("PasswordPartial");
		}

		[EnsureInPortal]
		[UnitOfWorkAction]
		[HttpGet]
		public JsonResult GetSettings()
		{
			var settings = _settingsViewModelFactory.CreateViewModel();
			return Json(settings, JsonRequestBehavior.AllowGet);
		}

		[UnitOfWorkAction]
		[HttpPut]
		public void UpdateCulture(int lcid)
		{
			var culture = lcid > 0 ? CultureInfo.GetCultureInfo(lcid) : null;
			_personPersister.UpdateCulture(_loggedOnUser.CurrentUser(), culture);
		}

		[UnitOfWorkAction]
		[HttpPut]
		public void UpdateUiCulture(int lcid)
		{
			var culture = lcid > 0 ? CultureInfo.GetCultureInfo(lcid) : null;
			_personPersister.UpdateUICulture(_loggedOnUser.CurrentUser(), culture);
		}

		[UnitOfWorkAction]
		[HttpPostOrPut]
		[TenantUnitOfWork]
		public virtual JsonResult ChangePassword(ChangePasswordViewModel model)
		{
			var loggedOnUser = _loggedOnUser.CurrentUser();
			var ret = new ChangePasswordResultInfo();
			try
			{
				_changePersonPassword.Modify(loggedOnUser.Id.Value, model.OldPassword, model.NewPassword);
				ret.IsSuccessful = true;
			}
			catch (HttpException httpException)
			{
				if (httpException.GetHttpCode() != 403)
				{
					ret.IsAuthenticationSuccessful = true;
				}
				Response.TrySkipIisCustomErrors = true;
				Response.StatusCode = 400;
			}
			return Json(ret);
		}

		[UnitOfWorkAction]
		[HttpPostOrPut]
		[ApplicationFunction(DefinedRaptorApplicationFunctionPaths.ShareCalendar)]
		public JsonResult SetCalendarLinkStatus(bool isActive)
		{
			var settings = _calendarLinkSettingsPersisterAndProvider.Persist(new CalendarLinkSettings {IsActive = isActive});
			return Json(_calendarLinkViewModelFactory.CreateViewModel(settings, "SetCalendarLinkStatus"));
		}

		[UnitOfWorkAction]
		[HttpGet]
		[ApplicationFunction(DefinedRaptorApplicationFunctionPaths.ShareCalendar)]
		public JsonResult CalendarLinkStatus()
		{
			var settings = _calendarLinkSettingsPersisterAndProvider.Get();
			return Json(_calendarLinkViewModelFactory.CreateViewModel(settings, "CalendarLinkStatus"),
			            JsonRequestBehavior.AllowGet);
		}

		[UnitOfWorkAction]
		[HttpPut]
		public void UpdateNameFormat(int nameFormatId)
		{
			_nameFormatSettingsPersisterAndProvider.Persist(new NameFormatSettings { NameFormatId = nameFormatId });
		}
	}
}