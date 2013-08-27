﻿using System.Globalization;
using System.Web.Mvc;
using AutoMapper;
using Teleopti.Ccc.Domain.Security.AuthorizationData;
using Teleopti.Ccc.Domain.SystemSetting.GlobalSetting;
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
		private readonly IMappingEngine _mapper;
		private readonly ILoggedOnUser _loggedOnUser;
		private readonly IModifyPassword _modifyPassword;
		private readonly IPersonPersister _personPersister;
		private readonly ISettingsPermissionViewModelFactory _settingsPermissionViewModelFactory;
		private readonly ICalendarLinkSettingsPersisterAndProvider _calendarLinkSettingsPersisterAndProvider;
		private readonly ICalendarLinkViewModelFactory _calendarLinkViewModelFactory;

		public SettingsController(IMappingEngine mapper,
		                          ILoggedOnUser loggedOnUser,
		                          IModifyPassword modifyPassword,
		                          IPersonPersister personPersister,
		                          ISettingsPermissionViewModelFactory settingsPermissionViewModelFactory,
		                          ICalendarLinkSettingsPersisterAndProvider calendarLinkSettingsPersisterAndProvider,
		                          ICalendarLinkViewModelFactory calendarLinkViewModelFactory)
		{
			_mapper = mapper;
			_loggedOnUser = loggedOnUser;
			_modifyPassword = modifyPassword;
			_personPersister = personPersister;
			_settingsPermissionViewModelFactory = settingsPermissionViewModelFactory;
			_calendarLinkSettingsPersisterAndProvider = calendarLinkSettingsPersisterAndProvider;
			_calendarLinkViewModelFactory = calendarLinkViewModelFactory;
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
		public JsonResult Cultures()
		{
			var settings = _mapper.Map<IPerson, SettingsViewModel>(_loggedOnUser.CurrentUser());
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
		public JsonResult ChangePassword(ChangePasswordViewModel model)
		{
			var result = _modifyPassword.Change(_loggedOnUser.CurrentUser(), model.OldPassword, model.NewPassword);
			if (!result.IsSuccessful)
			{
				Response.TrySkipIisCustomErrors = true;
				Response.StatusCode = 400;
			}
			return Json(result);
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
	}
}