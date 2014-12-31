using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.FeatureFlags;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Web.Areas.MyTime.Core.Portal.DataProvider;
using Teleopti.Ccc.Web.Areas.MyTime.Models.BadgeLeaderBoardReport;
using Teleopti.Ccc.Web.Core;
using Teleopti.Interfaces.Domain;
using Teleopti.Ccc.Infrastructure.Toggle;

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
		private readonly IAgentBadgeWithRankRepository _agentBadgeWithRankRepository;
		private readonly IPermissionProvider _permissionProvider;
		private readonly IPersonRepository _personRepository;
		private readonly IBadgeSettingProvider _agentBadgeSettingsProvider;
		private readonly IPersonNameProvider _personNameProvider;
		private readonly ISiteRepository _siteRepository;
		private readonly ITeamRepository _teamRepository;
		private readonly IToggleManager _toggleManager;
		private bool toggleEnabled;

		private IPerson[] permittedPersonList;

		public LeaderboardAgentBadgeProvider(IAgentBadgeRepository agentBadgeRepository,
			IAgentBadgeWithRankRepository agentBadgeWithRankRepository,
			IPermissionProvider permissionProvider,
			IPersonRepository personRepository, IBadgeSettingProvider agentBadgeSettingsProvider,
			IPersonNameProvider personNameProvider,
			ISiteRepository siteRepository, ITeamRepository teamRepository,
			IToggleManager toggleManager)
		{
			_agentBadgeRepository = agentBadgeRepository;
			_agentBadgeWithRankRepository = agentBadgeWithRankRepository;
			_permissionProvider = permissionProvider;
			_personRepository = personRepository;
			_agentBadgeSettingsProvider = agentBadgeSettingsProvider;
			_personNameProvider = personNameProvider;
			_siteRepository = siteRepository;
			_teamRepository = teamRepository;
			_toggleManager = toggleManager;
		}

		public IEnumerable<AgentBadgeOverview> GetPermittedAgents(string functionPath, LeaderboardQuery query)
		{
			var setting = _agentBadgeSettingsProvider.GetBadgeSettings();
			var queryDate = query.Date;
			IEnumerable<AgentWithBadge> permittedAgentBadgeList = new List<AgentWithBadge>();
			toggleEnabled = _toggleManager.IsEnabled(Toggles.Gamification_NewBadgeCalculation_31185);

			switch (query.Type)
			{
				case LeadboardQueryType.Everyone:
				case LeadboardQueryType.MyOwn:
				{
					// Handling of Everyone and MyOwn is different with Site or Team, this is to get best performance.
					permittedAgentBadgeList = getAllPermittedAgentBadgeList(functionPath, setting, queryDate);
					break;
				}
				case LeadboardQueryType.Site:
				{
					var site = _siteRepository.Get(query.SelectedId);
					var teamsInSite = site.TeamCollection;
					var agentsInSite = new List<IPerson>();
					teamsInSite.ForEach(
						x => agentsInSite.AddRange(_personRepository.FindPeopleBelongTeam(x, new DateOnlyPeriod(queryDate.AddDays(-1), queryDate))));

					permittedAgentBadgeList = getPermittedAgentBadgeList(functionPath, agentsInSite, queryDate, setting);
					break;
				}
				case LeadboardQueryType.Team:
				{
					var team = _teamRepository.Get(query.SelectedId);
					var agentsInTeam = _personRepository.FindPeopleBelongTeam(team, new DateOnlyPeriod(queryDate.AddDays(-1), queryDate));

					permittedAgentBadgeList = getPermittedAgentBadgeList(functionPath, agentsInTeam, queryDate, setting);
					break;
				}
			}

			return getPermittedAgentOverviews(permittedAgentBadgeList);
		}

		//this is only used for handling cases of Everyone and MyOwn to get best performance, to be confirmed for why
		//returns total gold/silver/bronze badge number for each agent.
		private IEnumerable<AgentWithBadge> getAllPermittedAgentBadgeList(string functionPath, IAgentBadgeSettings setting, DateOnly date)
		{
			//get agents' bronze badges and convert to silver/gold badges based on conversion rate
			//this is for the case of badge calculation method1: accumulative awarding
			var agentBadges = _agentBadgeRepository.GetAllAgentBadges() ?? new IAgentBadge[] {};
			var agentWithBadges =
				getAgentWithBadges(agentBadges, setting.SilverToBronzeBadgeRate, setting.GoldToSilverBadgeRate).ToList();
		
			//get agents' bronze/silver/gold badges that are directly awarded without converion from bronze badge
			//this is for the case of badge calcualtion method2: direct awarding
			if (toggleEnabled)
			{
				var agentBadgeWithRank = _agentBadgeWithRankRepository.GetAllAgentBadges() ?? new IAgentBadgeWithRank[] {};
				var agentWithRankedBadges = getAgentWithBadges(agentBadgeWithRank).ToList();
				agentWithBadges = mergeAgentWithBadges(agentWithBadges, agentWithRankedBadges).ToList();
			}

			var personGuidList = agentWithBadges.Select(item => item.Person).ToList();
			var personList = _personRepository.FindPeople(personGuidList);
			permittedPersonList = getPermittedAgents(functionPath, personList, date);

			var result = from a in agentWithBadges
				join p in permittedPersonList on a.Person equals p.Id
				select a;
			return result;
		}

		//combine badge number by gold/silver/bronze regardless of badge types£¨call, adherence, AHT)
		//from using method1 and method2 for each specific agent to include total badges
		private IEnumerable<AgentWithBadge> mergeAgentWithBadges(IEnumerable<AgentWithBadge> agentWithBadges1,
			IEnumerable<AgentWithBadge> agentWithBadges2)
		{
			var list1 = agentWithBadges1.ToList();
			var list2 = agentWithBadges2.ToList();
			var result = list1.Concat(list2)
				.GroupBy(badge => badge.Person)
				.Select(group => new AgentWithBadge
				{
					Person = group.Key,
					BronzeBadgeAmount = group.Sum(x => x.BronzeBadgeAmount),
					SilverBadgeAmount = group.Sum(x => x.SilverBadgeAmount),
					GoldBadgeAmount = group.Sum(x => x.GoldBadgeAmount)
				});
			return result;
		}

		//gets all badges (gold/silver/bronze) for each agent by aggregating all types of badges
 		//returns total gold/silver/bronze badge number for each agent.
		private IEnumerable<AgentWithBadge> getPermittedAgentBadgeList(string functionPath, IEnumerable<IPerson> allAgents,
			DateOnly date, IAgentBadgeSettings setting)
		{
			permittedPersonList = getPermittedAgents(functionPath, allAgents, date);
			var permittedPersonIdList = permittedPersonList.Select(x => x.Id.Value).ToList();

			var agentBadges = _agentBadgeRepository.Find(permittedPersonIdList) ?? new List<IAgentBadge>();
			var agentWithBadges = getAgentWithBadges(agentBadges, setting.SilverToBronzeBadgeRate, setting.GoldToSilverBadgeRate);
			
			if (toggleEnabled)
			{				
				var agentBadgesWithRank = _agentBadgeWithRankRepository.Find(permittedPersonIdList) ??
				                          new List<IAgentBadgeWithRank>();
				var agentWithRankedBadges = getAgentWithBadges(agentBadgesWithRank).ToList();
				return mergeAgentWithBadges(agentWithBadges, agentWithRankedBadges);
			}
			return agentWithBadges;		
		}

		private static IEnumerable<AgentWithBadge> getAgentWithBadges(IEnumerable<IAgentBadge> agentBadges,
			int silverToBronzeRate, int goldToSilverRate)
		{
			var result = agentBadges.GroupBy(x => x.Person)
				.Select(group => new AgentWithBadge
					{
						Person = group.Key,
						GoldBadgeAmount = group.Sum(x => x.GetGoldBadge(silverToBronzeRate, goldToSilverRate)),
						SilverBadgeAmount = group.Sum(x => x.GetSilverBadge(silverToBronzeRate, goldToSilverRate)),
						BronzeBadgeAmount = group.Sum(x => x.GetBronzeBadge(silverToBronzeRate, goldToSilverRate))
					});

			return result;
		}

		private static IEnumerable<AgentWithBadge> getAgentWithBadges(IEnumerable<IAgentBadgeWithRank> agentBadges)
		{
			var result = agentBadges.GroupBy(x => x.Person)
				.Select(group => new AgentWithBadge
					{
						Person = group.Key,
						GoldBadgeAmount = group.Sum(x => x.GoldBadgeAmount),
						SilverBadgeAmount = group.Sum(x => x.SilverBadgeAmount),
						BronzeBadgeAmount = group.Sum(x => x.BronzeBadgeAmount)
					});

			return result;
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
