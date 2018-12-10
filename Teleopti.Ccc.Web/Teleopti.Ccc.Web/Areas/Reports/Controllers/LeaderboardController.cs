using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Http;
using System.Web.Http.Results;
using Teleopti.Ccc.Domain.Aop;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Web.Areas.MyTime.Models.BadgeLeaderBoardReport;
using Teleopti.Ccc.Web.Areas.Reports.Core;
using Teleopti.Ccc.Web.Areas.Reports.Models;
using Teleopti.Ccc.Web.Areas.Search.Controllers;


namespace Teleopti.Ccc.Web.Areas.Reports.Controllers
{
	public class LeaderboardController:ApiController
	{
		private readonly ILoggedOnUser _loggonUser;
		private readonly IAgentBadgeProvider _agentBadgeProvider;

		public LeaderboardController(ILoggedOnUser loggonUser, IAgentBadgeProvider agentBadgeProvider)
		{
			_loggonUser = loggonUser;
			_agentBadgeProvider = agentBadgeProvider;
		}
		[UnitOfWork, HttpGet, Route("api/Reports/SearchLeaderboard")]
		public virtual JsonResult<LeaderboardViewModel> SearchLeaderboard(string keyword, DateTime? startDate = null, DateTime? endDate = null)
		{
			var currentDate = DateOnly.Today;

			var period = startDate.HasValue && endDate.HasValue
				? new DateOnlyPeriod(new DateOnly(startDate.Value), new DateOnly(endDate.Value))
				: new DateOnlyPeriod(new DateOnly(1900, 1, 1), DateOnly.Today);
			
			var myTeam = _loggonUser.CurrentUser().MyTeam(currentDate);
			var result = new List<AgentBadgeOverview>();
			if (string.IsNullOrEmpty(keyword) && myTeam == null)
			{
				result.AddRange(
					_agentBadgeProvider.GetAllAgentBadges(currentDate, period)
						.OrderByDescending(x => x.Gold)
						.ThenByDescending(x => x.Silver)
						.ThenByDescending(x => x.Bronze));
				return Json(new LeaderboardViewModel
				{
					Keyword = "",
					AgentBadges = result
				});
			}
			keyword = SearchTermParser.KeywordWithDefault(keyword, currentDate, myTeam);
			var criteriaDic = SearchTermParser.Parse(keyword);
			result.AddRange(
				_agentBadgeProvider.GetAgentBadge(criteriaDic, currentDate,period)
					.OrderByDescending(x => x.Gold)
					.ThenByDescending(x => x.Silver)
					.ThenByDescending(x => x.Bronze));
			return Json(new LeaderboardViewModel
			{
				Keyword = keyword,
				AgentBadges = result
			});
		} 
	}
}