using System.Globalization;
using System.Web.Mvc;
using AutoMapper;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.SystemSetting.GlobalSetting;
using Teleopti.Ccc.Web.Areas.MyTime.Core;
using Teleopti.Ccc.Web.Areas.MyTime.Core.Filters;
using Teleopti.Ccc.Web.Areas.MyTime.Core.Settings;
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
		private readonly IPersonalSettingDataRepository _personalSettingDataRepository;
		private readonly ICurrentDataSource _currentDataSource;

		public SettingsController(IMappingEngine mapper, ILoggedOnUser loggedOnUser, IModifyPassword modifyPassword, IPersonPersister personPersister, IPersonalSettingDataRepository personalSettingDataRepository, ICurrentDataSource currentDataSource)
		{
			_mapper = mapper;
			_loggedOnUser = loggedOnUser;
			_modifyPassword = modifyPassword;
			_personPersister = personPersister;
			_personalSettingDataRepository = personalSettingDataRepository;
			_currentDataSource = currentDataSource;
		}

		[EnsureInPortal]
		[UnitOfWorkAction]
		[HttpGet]
		public ViewResult Index()
		{
			return View("RegionalSettingsPartial");
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
			var test = _mapper.Map<IPerson, SettingsViewModel>(_loggedOnUser.CurrentUser());
			return Json(test, JsonRequestBehavior.AllowGet);
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
		public JsonResult ActivateCalendarLink(bool isActive)
		{
			var setting = _personalSettingDataRepository.FindValueByKey("CalendarLinkSettings", new CalendarLinkSettings());
			setting.IsActive = isActive;
			_personalSettingDataRepository.PersistSettingValue(setting);
			var requestUrl = Request.Url.OriginalString;
			var dataSourceName = _currentDataSource.CurrentName();
			var url =
				requestUrl.Substring(0, requestUrl.LastIndexOf("Settings/ActivateCalendarLink", System.StringComparison.Ordinal)) +
				"Calendar/Get/" + dataSourceName + "/" +
				_loggedOnUser.CurrentUser().Id.Value;
			return Json(url);
		}
	}
}