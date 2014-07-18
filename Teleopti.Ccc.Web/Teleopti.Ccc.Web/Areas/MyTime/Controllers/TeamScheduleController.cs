using System;
using System.Web.Mvc;
using Teleopti.Ccc.Domain.Security.AuthorizationData;
using Teleopti.Ccc.Web.Areas.MyTime.Core.Filters;
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
		private readonly IDefaultTeamProvider _defaultTeamProvider;

		public TeamScheduleController(ITeamScheduleViewModelFactory teamScheduleViewModelFactory, IDefaultTeamProvider defaultTeamProvider)
		{
			_teamScheduleViewModelFactory = teamScheduleViewModelFactory;
			_defaultTeamProvider = defaultTeamProvider;
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
					return View("NoTeamsPartial", model: date.Value.Date.ToString("yyyyMMdd"));

				id = defaultTeam.Id;
			}
			return View("TeamSchedulePartial", _teamScheduleViewModelFactory.CreateViewModel(date.Value, id.Value));
		}
	}
}