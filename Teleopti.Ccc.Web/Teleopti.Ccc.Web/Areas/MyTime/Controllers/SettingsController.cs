using System.Globalization;
using System.Web.Mvc;
using AutoMapper;
using Teleopti.Ccc.Web.Areas.MyTime.Models.Settings;
using Teleopti.Ccc.Web.Core.RequestContext;
using Teleopti.Ccc.Web.Filters;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Web.Areas.MyTime.Controllers
{
	public class SettingsController : Controller
	{
		private readonly IMappingEngine _mapper;
		private readonly ILoggedOnUser _loggedOnUser;

		public SettingsController(IMappingEngine mapper, ILoggedOnUser loggedOnUser)
		{
			_mapper = mapper;
			_loggedOnUser = loggedOnUser;
		}

		[UnitOfWorkAction]
		[HttpGet]
		public ViewResult Index()
		{
			var viewModel = _mapper.Map<IPerson, SettingsViewModel>(_loggedOnUser.CurrentUser());
			return View("RegionalSettingsPartial", viewModel);
		}

		[HttpGet]
		public ViewResult Password()
		{
			return View("PasswordPartial");
		}

		[UnitOfWorkAction]
		[HttpPut]
		public void UpdateCulture(int lcid)
		{
			var culture = lcid > 0 ? CultureInfo.GetCultureInfo(lcid) : null;
			_loggedOnUser.CurrentUser().PermissionInformation.SetCulture(culture);
		}

		[UnitOfWorkAction]
		[HttpPut]
		public void UpdateUiCulture(int lcid)
		{
			var culture = lcid > 0 ? CultureInfo.GetCultureInfo(lcid) : null;
			_loggedOnUser.CurrentUser().PermissionInformation.SetUICulture(culture);
		}
	}
}