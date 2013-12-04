using System;
using System.Web.Mvc;
using Teleopti.Ccc.Domain.Security.AuthorizationData;
using Teleopti.Ccc.Web.Areas.MyTime.Core.Common.DataProvider;
using Teleopti.Ccc.Web.Areas.MyTime.Core.Common.ViewModelFactory;
using Teleopti.Ccc.Web.Areas.MyTime.Core.Filters;
using Teleopti.Ccc.Web.Areas.MyTime.Core.TeamSchedule.ViewModelFactory;
using Teleopti.Ccc.Web.Filters;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Web.Areas.MyTime.Controllers
{
	[ApplicationFunction(DefinedRaptorApplicationFunctionPaths.TeamSchedule)]
	public class TeamScheduleController : Controller
	{
		private readonly ITeamScheduleViewModelFactory _teamScheduleViewModelFactory;
		private readonly IDefaultTeamProvider _defaultTeamProvider;
		private readonly ITeamViewModelFactory _teamViewModelFactory;

		public TeamScheduleController(ITeamScheduleViewModelFactory teamScheduleViewModelFactory, IDefaultTeamProvider defaultTeamProvider, ITeamViewModelFactory teamViewModelFactory)
		{
			_teamScheduleViewModelFactory = teamScheduleViewModelFactory;
			_defaultTeamProvider = defaultTeamProvider;
			_teamViewModelFactory = teamViewModelFactory;
		}

		[EnsureInPortal]
		[UnitOfWorkAction]
		public ViewResult Index(DateOnly? date, Guid? id)
		{
			if (!date.HasValue)
				date = DateOnly.Today;
			if (!id.HasValue)
			{
				var defaultTeam = _defaultTeamProvider.DefaultTeam(date.Value);
				if (defaultTeam == null)
					return View("NoTeamsPartial");

				id = defaultTeam.Id;
			}
			return View("TeamSchedulePartial", _teamScheduleViewModelFactory.CreateViewModel(date.Value, id.Value));
		}

		[UnitOfWorkAction]
		public JsonResult Teams(DateOnly? date)
		{
			if (!date.HasValue)
				date = DateOnly.Today;
			return Json(_teamViewModelFactory.CreateTeamOrGroupOptionsViewModel(date.Value), JsonRequestBehavior.AllowGet);
		}
	}
}