using System.Web.Mvc;
using Teleopti.Ccc.Domain.Aop;
using Teleopti.Ccc.Domain.Common.Time;
using Teleopti.Ccc.Domain.Security.AuthorizationData;
using Teleopti.Ccc.UserTexts;
using Teleopti.Ccc.Web.Areas.MyTime.Core.Common.ViewModelFactory;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Web.Areas.MyTime.Controllers
{
	public class TeamController : Controller
	{
		private readonly ITeamViewModelFactory _teamViewModelFactory;
		private readonly INow _now;

		public TeamController(ITeamViewModelFactory teamViewModelFactory, INow now)
		{
			_teamViewModelFactory = teamViewModelFactory;
			_now = now;
		}

		[UnitOfWork]
		public virtual JsonResult TeamsAndOrGroupings(DateOnly? date)
		{
			if (!date.HasValue)
				date = _now.LocalDateOnly();
			return Json(
				_teamViewModelFactory.CreateTeamOrGroupOptionsViewModel(date.Value),
				JsonRequestBehavior.AllowGet);
		}

		[UnitOfWork]
		public virtual JsonResult TeamsForShiftTrade(DateOnly? date)
		{
			if (!date.HasValue)
				date = _now.LocalDateOnly();
			return
				Json(
					_teamViewModelFactory.CreateTeamOptionsViewModel(date.Value, DefinedRaptorApplicationFunctionPaths.ShiftTradeRequestsWeb),
					JsonRequestBehavior.AllowGet);
		}

		[UnitOfWork]
		[HttpGet]
		public virtual JsonResult TeamsAndGroupsWithAllTeam(DateOnly? date)
		{
			if (!date.HasValue)
				date = _now.LocalDateOnly();
			return Json(
				new
				{
					teams = _teamViewModelFactory.CreateTeamOrGroupOptionsViewModel(date.Value),
					allTeam = new { id = "allTeams", text = Resources.AllPermittedTeamsToMakeShiftTradeWith }
				}, JsonRequestBehavior.AllowGet);
		}
	}
}