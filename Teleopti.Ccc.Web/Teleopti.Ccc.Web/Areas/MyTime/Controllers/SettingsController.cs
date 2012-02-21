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
		public ViewResult Index()
		{
			var viewModel = _mapper.Map<IPerson, SettingsViewModel>(_loggedOnUser.CurrentUser());
			return View("SettingsPartial", viewModel);
		}
	}
}