using System;
using System.Web.Mvc;
using Teleopti.Ccc.Domain.Aop;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Security.AuthorizationData;
using Teleopti.Ccc.Web.Areas.MyTime.Core.Filters;
using Teleopti.Ccc.Web.Areas.MyTime.Core.TeamSchedule.ViewModelFactory;
using Teleopti.Ccc.Web.Filters;


namespace Teleopti.Ccc.Web.Areas.MyTime.Controllers
{
	[ApplicationFunction(DefinedRaptorApplicationFunctionPaths.TeamSchedule)]
	public class TeamScheduleController : Controller
	{
		private readonly ITeamSchedulePermissionViewModelFactory _teamSchedulePermissionViewModelFactory;

		public TeamScheduleController(ITeamSchedulePermissionViewModelFactory teamSchedulePermissionViewModelFactory)
		{
			_teamSchedulePermissionViewModelFactory = teamSchedulePermissionViewModelFactory;
		}

		[HttpGet]
		[EnsureInPortal]
		[UnitOfWork]
		public virtual ViewResult Index(DateOnly? date, Guid? id)
		{
			return View("TeamSchedulePartial", _teamSchedulePermissionViewModelFactory.CreateTeamSchedulePermissionViewModel());
		}

		[HttpGet]
		[EnsureInPortal]
		[UnitOfWork]
		public virtual ViewResult NewIndex(DateOnly? date, Guid? id)
		{
			return View("NewTeamSchedulePartial", _teamSchedulePermissionViewModelFactory.CreateTeamSchedulePermissionViewModel());
		}
	}
}