using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Ccc.Web.Areas.MyTime.Core.Common.DataProvider;
using Teleopti.Ccc.Web.Areas.MyTime.Models.BadgeLeaderBoardReport;

using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

namespace Teleopti.Ccc.Web.Areas.MyTime.Core.BadgeLeaderBoardReport.ViewModelFactory
{
	public class BadgeLeaderBoardReportOptionFactory :IBadgeLeaderBoardReportOptionFactory
	{
		private readonly ITeamProvider _teamProvider;
		private readonly IAuthorization _authorization;
		private readonly ILoggedOnUser _currentLoggedOnUser;

		public BadgeLeaderBoardReportOptionFactory(ITeamProvider teamProvider, IAuthorization authorization, ILoggedOnUser currentLoggedOnUser)
		{
			_teamProvider = teamProvider;
			_authorization = authorization;
			_currentLoggedOnUser = currentLoggedOnUser;
		}

		public dynamic CreateLeaderboardOptions(DateOnly date, string functionPath)
		{
			var myTeam = _currentLoggedOnUser.CurrentUser().MyTeam(date);
			var teams = _teamProvider.GetPermittedTeams(date, functionPath).ToArray();
			var isMyTeamPermitted = teams.Contains(myTeam);
			var sites = teams.GroupBy(t => t.Site)
				.OrderBy(s => s.Key.Description.Name);

			var options = new List<dynamic>
			{
				new
				{
					id = Guid.Empty,
					text = UserTexts.Resources.Everyone,
					type = LeadboardQueryType.Everyone
				}
			};
			sites.ForEach(s =>
			{
				if (_authorization.IsPermitted(functionPath, date, s.Key))
				{
					options.Add(new
					{
						id = s.Key.Id,
						text = s.Key.Description.Name,
						type = LeadboardQueryType.Site
					});
				}

				var teamOptions = s.Select(t => new
				{
					id = t.Id,
					text = t.Description.Name,
					type = LeadboardQueryType.Team
				});
				options.AddRange(teamOptions);
			});
			return new { options, defaultOptionId = isMyTeamPermitted ? myTeam.Id : Guid.Empty };
		}
	}
}