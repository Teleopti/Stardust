using System;
using System.Collections.Generic;
using System.Linq;
using DotNetOpenAuth.Messaging;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Web.Areas.MyTime.Controllers;
using Teleopti.Ccc.Web.Areas.MyTime.Core.Portal.DataProvider;
using Teleopti.Ccc.Web.Areas.MyTime.Models.BadgeLeaderBoardReport;
using Teleopti.Ccc.Web.Core;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Web.Areas.MyTime.Core.Common.DataProvider
{
	public class AgentBadgeProvider: IAgentBadgeProvider
	{
		private readonly IAgentBadgeRepository _agentBadgeRepository;
		private readonly IPermissionProvider _permissionProvider;
		private readonly IPersonRepository _personRepository;
		private readonly IBadgeSettingProvider _agentBadgeSettingsProvider;
		private readonly IPersonNameProvider _personNameProvider;
		private readonly ISiteRepository _siteRepository;
		private readonly ITeamRepository _teamRepository;

		public AgentBadgeProvider(IAgentBadgeRepository agentBadgeRepository, IPermissionProvider permissionProvider, IPersonRepository personRepository, IBadgeSettingProvider agentBadgeSettingsProvider, IPersonNameProvider personNameProvider, ISiteRepository siteRepository, ITeamRepository teamRepository)
		{
			_agentBadgeRepository = agentBadgeRepository;
			_permissionProvider = permissionProvider;
			_personRepository = personRepository;
			_agentBadgeSettingsProvider = agentBadgeSettingsProvider;
			_personNameProvider = personNameProvider;
			_siteRepository = siteRepository;
			_teamRepository = teamRepository;
		}

		public IEnumerable<AgentBadgeOverview> GetPermittedAgents(string functionPath, LeaderboardQuery query)
		{
			var date = query.Date;
			var permittedPersonList = new IPerson[] { };
			var permittedAgentBadgeList = new IAgentBadge[] { };

			switch (query.Type)
			{
				case LeadboardQueryType.Everyone:
				case LeadboardQueryType.MyOwn:
				{
					var agentBadges = _agentBadgeRepository.GetAllAgentBadges() ?? new IAgentBadge[] { };
					var personGuidList = agentBadges.Select(item => item.Person).ToList();

					var personList = _personRepository.FindPeople(personGuidList);
					permittedPersonList = getPermittedAgents(functionPath, personList, date);

					permittedAgentBadgeList = (from a in agentBadges
						join p in permittedPersonList on a.Person equals p.Id
						select a).ToArray();

				}
					break;
				case LeadboardQueryType.Site:
				{
					var site = _siteRepository.Get(query.SelectedId);
					var teamsInSite = site.TeamCollection;
					var agentsInSite = new List<IPerson>();
					teamsInSite.ForEach(
						x => agentsInSite.AddRange(_personRepository.FindPeopleBelongTeam(x, new DateOnlyPeriod(date.AddDays(-1), date))));
					permittedPersonList = getPermittedAgents(functionPath, agentsInSite, date);
					permittedAgentBadgeList = _agentBadgeRepository.Find(permittedPersonList.Select(x => x.Id.Value)).ToArray();
				
				}
					break;
				case LeadboardQueryType.Team:	
				{
					var team = _teamRepository.Get(query.SelectedId);                      
					var agentsInTeam = _personRepository.FindPeopleBelongTeam(team, new DateOnlyPeriod(date.AddDays(-1), date));
					permittedPersonList = getPermittedAgents(functionPath, agentsInTeam, date);
					permittedAgentBadgeList = _agentBadgeRepository.Find(permittedPersonList.Select(x => x.Id.Value)).ToArray();
				}
					break;
			}

			return getPermittedAgentOverviews(permittedPersonList, permittedAgentBadgeList);	
		}

		private IEnumerable<AgentBadgeOverview> getPermittedAgentOverviews(IEnumerable<IPerson> permittedPersonList, IAgentBadge[] permittedAgentBadgeList)
		{
			var agentBadgeList = new List<AgentBadgeOverview>();
			var setting = _agentBadgeSettingsProvider.GetBadgeSettings();
			foreach (var p in permittedPersonList)
			{
				var agentOverviewItem = new AgentBadgeOverview {AgentName = _personNameProvider.BuildNameFromSetting(p.Name)};
				foreach (var agentBadge in permittedAgentBadgeList.Where(agentBadge => p.Id == agentBadge.Person))
				{
					agentOverviewItem.Gold += agentBadge.GetGoldBadge(setting.SilverToBronzeBadgeRate, setting.GoldToSilverBadgeRate);
					agentOverviewItem.Silver += agentBadge.GetSilverBadge(setting.SilverToBronzeBadgeRate, setting.GoldToSilverBadgeRate);
					agentOverviewItem.Bronze += agentBadge.GetBronzeBadge(setting.SilverToBronzeBadgeRate, setting.GoldToSilverBadgeRate);
				}
				agentBadgeList.Add(agentOverviewItem);
			}
			return agentBadgeList;
		}

		private IPerson[] getPermittedAgents(string functionPath, IEnumerable<IPerson> personList, DateOnly date)
		{
			return (from t in personList
				where _permissionProvider.HasPersonPermission(functionPath, date, t)
				select t).ToArray();
		}
	}
}


