using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Web.Areas.MyTime.Core.Portal.DataProvider;
using Teleopti.Ccc.Web.Areas.MyTime.Models.BadgeLeaderBoardReport;
using Teleopti.Ccc.Web.Core;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Web.Areas.MyTime.Core.Common.DataProvider
{
	public class LeaderboardAgentBadgeProvider : ILeaderboardAgentBadgeProvider
	{
		private struct AgentWithBadge
		{
			public Guid Person;
			public int BronzeBadgeAmount;
			public int SilverBadgeAmount;
			public int GoldBadgeAmount;
		}

		private readonly IAgentBadgeRepository _agentBadgeRepository;
		private readonly IPermissionProvider _permissionProvider;
		private readonly IPersonRepository _personRepository;
		private readonly IBadgeSettingProvider _agentBadgeSettingsProvider;
		private readonly IPersonNameProvider _personNameProvider;
		private readonly ISiteRepository _siteRepository;
		private readonly ITeamRepository _teamRepository;

		private IPerson[] permittedPersonList;

		public LeaderboardAgentBadgeProvider(IAgentBadgeRepository agentBadgeRepository,
			IPermissionProvider permissionProvider,
			IPersonRepository personRepository, IBadgeSettingProvider agentBadgeSettingsProvider,
			IPersonNameProvider personNameProvider,
			ISiteRepository siteRepository, ITeamRepository teamRepository)
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
			var setting = _agentBadgeSettingsProvider.GetBadgeSettings();
			var date = query.Date;
			IEnumerable<AgentWithBadge> permittedAgentBadgeList = new List<AgentWithBadge>();

			switch (query.Type)
			{
				case LeadboardQueryType.Everyone:
				case LeadboardQueryType.MyOwn:
				{
					var agentBadges = _agentBadgeRepository.GetAllAgentBadges() ?? new IAgentBadge[] {};
					var agentWithBadges =
						getAgentWithBadges(agentBadges, setting.SilverToBronzeBadgeRate, setting.GoldToSilverBadgeRate).ToList();
					var personGuidList = agentWithBadges.Select(item => item.Person).ToList();

					var personList = _personRepository.FindPeople(personGuidList);
					permittedPersonList = getPermittedAgents(functionPath, personList, date);

					permittedAgentBadgeList = (from a in agentWithBadges
						join p in permittedPersonList on a.Person equals p.Id
						select a).ToArray();
					break;
				}
				case LeadboardQueryType.Site:
				{
					var site = _siteRepository.Get(query.SelectedId);
					var teamsInSite = site.TeamCollection;
					var agentsInSite = new List<IPerson>();
					teamsInSite.ForEach(
						x => agentsInSite.AddRange(_personRepository.FindPeopleBelongTeam(x, new DateOnlyPeriod(date.AddDays(-1), date))));

					permittedAgentBadgeList = getPermittedAgentBadgeList(functionPath, agentsInSite, date, setting);
					break;
				}
				case LeadboardQueryType.Team:
				{
					var team = _teamRepository.Get(query.SelectedId);
					var agentsInTeam = _personRepository.FindPeopleBelongTeam(team, new DateOnlyPeriod(date.AddDays(-1), date));

					permittedAgentBadgeList = getPermittedAgentBadgeList(functionPath, agentsInTeam, date, setting);
					break;
				}
			}

			return getPermittedAgentOverviews(permittedAgentBadgeList);
		}

		private IEnumerable<AgentWithBadge> getPermittedAgentBadgeList(string functionPath, IEnumerable<IPerson> agentsInSite, DateOnly date, IAgentBadgeSettings setting)
		{
			permittedPersonList = getPermittedAgents(functionPath, agentsInSite, date);
			var agentBadges = _agentBadgeRepository.Find(permittedPersonList.Select(x => x.Id.Value)).ToArray();
			return getAgentWithBadges(agentBadges, setting.SilverToBronzeBadgeRate, setting.GoldToSilverBadgeRate);
		}

		private static IEnumerable<AgentWithBadge> getAgentWithBadges(IEnumerable<IAgentBadge> agentBadges,
			int silverToBronzeRate, int goldToSilverRate)
		{
			return agentBadges.Select(x => new AgentWithBadge
			{
				Person = x.Person,
				BronzeBadgeAmount = x.GetBronzeBadge(silverToBronzeRate, goldToSilverRate),
				SilverBadgeAmount = x.GetSilverBadge(silverToBronzeRate, goldToSilverRate),
				GoldBadgeAmount = x.GetGoldBadge(silverToBronzeRate, goldToSilverRate)
			});
		}

		private IEnumerable<AgentBadgeOverview> getPermittedAgentOverviews(IEnumerable<AgentWithBadge> permittedAgentBadgeList)
		{
			var permittedPersons = permittedPersonList.ToList();
			var dic = new Dictionary<Guid, AgentBadgeOverview>();
			foreach (var agentBadge in permittedAgentBadgeList)
			{
				var personId = agentBadge.Person;

				AgentBadgeOverview overview;
				if (dic.ContainsKey(personId))
				{
					overview = dic[personId];
				}
				else
				{
					overview = new AgentBadgeOverview
					{
						AgentName = _personNameProvider.BuildNameFromSetting(permittedPersons.Single(p => p.Id == personId).Name)
					};
					dic[personId] = overview;
				}

				overview.Gold += agentBadge.GoldBadgeAmount;
				overview.Silver += agentBadge.SilverBadgeAmount;
				overview.Bronze += agentBadge.BronzeBadgeAmount;
			}
			return dic.Values;
		}

		private IPerson[] getPermittedAgents(string functionPath, IEnumerable<IPerson> personList, DateOnly date)
		{
			return (from t in personList
				where _permissionProvider.HasPersonPermission(functionPath, date, t)
				select t).ToArray();
		}
	}
}
