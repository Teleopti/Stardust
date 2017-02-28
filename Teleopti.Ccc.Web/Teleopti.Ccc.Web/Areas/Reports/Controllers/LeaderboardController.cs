﻿using System;
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
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Web.Areas.Reports.Controllers
{
	public class LeaderboardController:ApiController
	{
		private readonly ILoggedOnUser _loggonUser;
		private readonly ISearchTermParser _parser;
		private readonly IAgentBadgeProvider _agentBadgeProvider;

		public LeaderboardController(ILoggedOnUser loggonUser, ISearchTermParser parser, IAgentBadgeProvider agentBadgeProvider)
		{
			_loggonUser = loggonUser;
			_parser = parser;
			_agentBadgeProvider = agentBadgeProvider;
		}
		[UnitOfWork, HttpGet, Route("api/Reports/SearchLeaderboard")]
		public virtual JsonResult<LeaderboardViewModel> SearchLeaderboard(string keyword, DateTime? startDate = null, DateTime? endDate = null)
		{
			var currentDate = DateOnly.Today;

			var period = (startDate.HasValue && endDate.HasValue)
				? new DateOnlyPeriod(startDate.Value.Year, startDate.Value.Month, startDate.Value.Day,endDate.Value.Year,endDate.Value.Month,endDate.Value.Day)
				: (DateOnlyPeriod?) null;


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
			keyword = _parser.Keyword(keyword, currentDate);
			var criteriaDic = _parser.Parse(keyword, currentDate);
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