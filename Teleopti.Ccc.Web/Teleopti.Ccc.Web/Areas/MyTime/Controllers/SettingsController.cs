using System.Globalization;
using System.Web;
using System.Web.Mvc;
using Teleopti.Ccc.Domain.Aop;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.MultiTenancy;
using Teleopti.Ccc.Domain.Security.AuthorizationData;
using Teleopti.Ccc.Domain.SystemSetting.GlobalSetting;
using Teleopti.Ccc.Infrastructure.MultiTenancy.Server;
using Teleopti.Ccc.Web.Areas.MyTime.Core;
using Teleopti.Ccc.Web.Areas.MyTime.Core.Filters;
using Teleopti.Ccc.Web.Areas.MyTime.Core.Settings.DataProvider;
using Teleopti.Ccc.Web.Areas.MyTime.Core.Settings.ViewModelFactory;
using Teleopti.Ccc.Web.Areas.MyTime.Models.Settings;
using Teleopti.Ccc.Web.Filters;

namespace Teleopti.Ccc.Web.Areas.MyTime.Controllers
{
	public class SettingsController : Controller
	{
		private readonly ILoggedOnUser _loggedOnUser;
		private readonly IPersonPersister _personPersister;
		private readonly ISettingsPermissionViewModelFactory _settingsPermissionViewModelFactory;
		private readonly ISettingsPersisterAndProvider<CalendarLinkSettings> _calendarLinkSettingsPersisterAndProvider;
		private readonly ICalendarLinkViewModelFactory _calendarLinkViewModelFactory;
		private readonly IPasswordManager _passwordManager;
		private readonly ICurrentTenant _currentTenant;
		private readonly ISettingsViewModelFactory _settingsViewModelFactory;
		private readonly ISettingsPersisterAndProvider<NameFormatSettings> _nameFormatSettingsPersisterAndProvider;

		public SettingsController(ILoggedOnUser loggedOnUser,
								  IPersonPersister personPersister,
								  ISettingsPermissionViewModelFactory settingsPermissionViewModelFactory,
										  ISettingsViewModelFactory settingsViewModelFactory,
										  ISettingsPersisterAndProvider<CalendarLinkSettings> calendarLinkSettingsPersisterAndProvider,
											ISettingsPersisterAndProvider<NameFormatSettings> nameFormatSettingsPersisterAndProvider,
											ICalendarLinkViewModelFactory calendarLinkViewModelFactory,
											IPasswordManager passwordManager,
											ICurrentTenant currentTenant)
		{
			_loggedOnUser = loggedOnUser;
			_personPersister = personPersister;
			_settingsPermissionViewModelFactory = settingsPermissionViewModelFactory;
			_settingsViewModelFactory = settingsViewModelFactory;
			_calendarLinkSettingsPersisterAndProvider = calendarLinkSettingsPersisterAndProvider;
			_nameFormatSettingsPersisterAndProvider = nameFormatSettingsPersisterAndProvider;
			_calendarLinkViewModelFactory = calendarLinkViewModelFactory;
			_passwordManager = passwordManager;
			_currentTenant = currentTenant;
		}

		[EnsureInPortal]
		[UnitOfWork]
		[HttpGet]
		public virtual ViewResult Index()
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
		[UnitOfWork]
		[HttpGet]
		public virtual JsonResult GetSettings()
		{
			var settings = _settingsViewModelFactory.CreateViewModel();
			return Json(settings, JsonRequestBehavior.AllowGet);
		}

		[UnitOfWork]
		[HttpPut]
		public virtual void UpdateCulture(int lcid)
		{
			var culture = lcid > 0 ? CultureInfo.GetCultureInfo(lcid) : null;
			_personPersister.UpdateCulture(_loggedOnUser.CurrentUser(), culture);
		}

		[UnitOfWork]
		[HttpPut]
		public virtual void UpdateUiCulture(int lcid)
		{
			var culture = lcid > 0 ? CultureInfo.GetCultureInfo(lcid) : null;
			_personPersister.UpdateUICulture(_loggedOnUser.CurrentUser(), culture);
		}

		[UnitOfWork]
		[HttpPostOrPut]
		[TenantUnitOfWork]
		[NoTenantAuthentication]
		public virtual JsonResult ChangePassword(ChangePasswordViewModel model)
		{
			var loggedOnUser = _loggedOnUser.CurrentUser();
			
			var ret = new ChangePasswordResultInfo();
			try
			{
				_passwordManager.Modify(loggedOnUser.Id.Value, model.OldPassword, model.NewPassword);
				ret.IsSuccessful = true;
				ret.IsAuthenticationSuccessful = true;

			}
			catch (HttpException httpException)
			{
				if (httpException.GetHttpCode() == 403)
				{
					ret.IsAuthenticationSuccessful = false;
				}
				else if (httpException.GetHttpCode() == 400)
				{
					ret.IsAuthenticationSuccessful = false;

					if (httpException.InnerException != null 
						&& httpException.InnerException.GetType() == typeof(PasswordStrengthException))
					{
						ret.ErrorCode = "Error_Policy";
					}					
				}
			}
			return Json(ret);
		}

		[UnitOfWork]
		[HttpPostOrPut]
		[ApplicationFunction(DefinedRaptorApplicationFunctionPaths.ShareCalendar)]
		public virtual JsonResult SetCalendarLinkStatus(bool isActive)
		{
			var settings = _calendarLinkSettingsPersisterAndProvider.Persist(new CalendarLinkSettings { IsActive = isActive });
			return Json(_calendarLinkViewModelFactory.CreateViewModel(settings, "SetCalendarLinkStatus"));
		}

		[UnitOfWork]
		[HttpGet]
		[ApplicationFunction(DefinedRaptorApplicationFunctionPaths.ShareCalendar)]
		public virtual JsonResult CalendarLinkStatus()
		{
			var settings = _calendarLinkSettingsPersisterAndProvider.Get();
			return Json(_calendarLinkViewModelFactory.CreateViewModel(settings, "CalendarLinkStatus"),
						JsonRequestBehavior.AllowGet);
		}

		[UnitOfWork]
		[HttpPut]
		public virtual void UpdateNameFormat(int nameFormatId)
		{
			_nameFormatSettingsPersisterAndProvider.Persist(new NameFormatSettings { NameFormatId = nameFormatId });
		}

		[TenantUnitOfWork, HttpGet]
		public virtual JsonResult MobileQRCodeUrl()
		{
			var url = _currentTenant.Current().GetApplicationConfig(TenantApplicationConfigKey.MobileQRCodeUrl) ?? string.Empty;
			return Json(url, JsonRequestBehavior.AllowGet);
		}
	}
}