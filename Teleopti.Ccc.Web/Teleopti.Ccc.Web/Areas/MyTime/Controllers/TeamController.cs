using System;
using System.Linq;
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
		private readonly ISiteViewModelFactory _siteViewModelFactory;
		private readonly INow _now;

		public TeamController(ITeamViewModelFactory teamViewModelFactory, INow now, ISiteViewModelFactory siteViewModelFactory)
		{
			_teamViewModelFactory = teamViewModelFactory;
			_now = now;
			_siteViewModelFactory = siteViewModelFactory;
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
		public virtual JsonResult TeamsForShiftTradeBoard(DateOnly? date)
		{
			if (!date.HasValue)
				date = _now.LocalDateOnly();
			return
				Json(
					_teamViewModelFactory.CreateTeamOptionsViewModel(date.Value, DefinedRaptorApplicationFunctionPaths.ShiftTradeBulletinBoard),
					JsonRequestBehavior.AllowGet);
		}

		[UnitOfWork]
		public virtual JsonResult SitesForShiftTradeBoard(DateOnly? date)
		{
			if (!date.HasValue)
				date = _now.LocalDateOnly();
			return
				Json(
					_siteViewModelFactory.CreateSiteOptionsViewModel(date.Value, DefinedRaptorApplicationFunctionPaths.ShiftTradeBulletinBoard),
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

		[UnitOfWork]
		[HttpPost]
		public virtual JsonResult GetTeamIds(string siteIds)
		{
			var allSiteIds = siteIds.Split(',').Select(siteId => new Guid(siteId)).ToList();

			return Json(_siteViewModelFactory.GetTeamIds(allSiteIds));
		}
	}
}