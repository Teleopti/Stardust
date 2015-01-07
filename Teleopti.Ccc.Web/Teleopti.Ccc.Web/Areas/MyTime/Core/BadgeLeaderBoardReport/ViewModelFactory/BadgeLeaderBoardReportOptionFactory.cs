using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Ccc.Web.Areas.MyTime.Core.Common.DataProvider;
using Teleopti.Ccc.Web.Areas.MyTime.Models.BadgeLeaderBoardReport;
using Teleopti.Interfaces.Domain;
using Teleopti.Ccc.Domain.Collection;

namespace Teleopti.Ccc.Web.Areas.MyTime.Core.BadgeLeaderBoardReport.ViewModelFactory
{
	public class BadgeLeaderBoardReportOptionFactory :IBadgeLeaderBoardReportOptionFactory
	{
		private readonly ITeamProvider _teamProvider;
		private readonly IPrincipalAuthorization _principalAuthorization;
		private readonly ILoggedOnUser _currentLoggedOnUser;

		public BadgeLeaderBoardReportOptionFactory(ITeamProvider teamProvider, IPrincipalAuthorization principalAuthorization, ILoggedOnUser currentLoggedOnUser)
		{
			_teamProvider = teamProvider;
			_principalAuthorization = principalAuthorization;
			_currentLoggedOnUser = currentLoggedOnUser;
		}

		public dynamic CreateLeaderboardOptions(DateOnly date, string functionPath)
		{
			var myTeam = _currentLoggedOnUser.CurrentUser().MyTeam(date);
			var teams = _teamProvider.GetPermittedTeams(date, functionPath).ToList();
			var sites = teams
				.Select(t => t.Site)
				.Distinct()
				.OrderBy(s => s.Description.Name);

			var options = new List<dynamic>();
			options.Add(new
			{
				id = Guid.Empty,
				text = UserTexts.Resources.Everyone,
				type = LeadboardQueryType.Everyone
			});
			sites.ForEach(s =>
			{
				if (_principalAuthorization.IsPermitted(functionPath, date, s))
				{
					options.Add(new
					{
						id = s.Id,
						text = s.Description.Name,
						type = LeadboardQueryType.Site
					});
				}

				var teamOptions = from t in teams
								  where t.Site == s
								  select new
								  {
									  id = t.Id,
									  text = t.Description.Name,
									  type = LeadboardQueryType.Team
								  };
				options.AddRange(teamOptions);
			});
			return new { options, defaultOptionId = myTeam == null ? Guid.Empty : myTeam.Id };
		}
	}
}