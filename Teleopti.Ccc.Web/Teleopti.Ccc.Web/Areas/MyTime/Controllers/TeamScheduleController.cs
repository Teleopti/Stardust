using System;
using System.Web.Mvc;
using Teleopti.Ccc.Domain.Security.AuthorizationData;
using Teleopti.Ccc.Web.Areas.MyTime.Core.TeamSchedule.DataProvider;
using Teleopti.Ccc.Web.Areas.MyTime.Core.TeamSchedule.ViewModelFactory;
using Teleopti.Ccc.Web.Filters;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Web.Areas.MyTime.Controllers
{
	[ApplicationFunction(DefinedRaptorApplicationFunctionPaths.TeamSchedule)]
	public class TeamScheduleController : Controller
	{
		private readonly ITeamScheduleViewModelFactory _teamScheduleViewModelFactory;
		private readonly IDefaultTeamCalculator _defaultTeamCalculator;

		public TeamScheduleController(ITeamScheduleViewModelFactory teamScheduleViewModelFactory, IDefaultTeamCalculator defaultTeamCalculator)
		{
			_teamScheduleViewModelFactory = teamScheduleViewModelFactory;
			_defaultTeamCalculator = defaultTeamCalculator;
		}

		[UnitOfWorkAction]
		public ViewResult Index(DateOnly? date, Guid? id)
		{
			if (!date.HasValue)
				date = DateOnly.Today;
			if (!id.HasValue)
				id = _defaultTeamCalculator.Calculate(date.Value).Id;
			return View("TeamSchedulePartial", _teamScheduleViewModelFactory.CreateViewModel(date.Value, id.Value));
		}

		[UnitOfWorkAction]
		public JsonResult Teams(DateOnly? date)
		{
			if (!date.HasValue)
				date = DateOnly.Today;
			return Json(_teamScheduleViewModelFactory.CreateTeamOptionsViewModel(date.Value), JsonRequestBehavior.AllowGet);
		}
	}
}