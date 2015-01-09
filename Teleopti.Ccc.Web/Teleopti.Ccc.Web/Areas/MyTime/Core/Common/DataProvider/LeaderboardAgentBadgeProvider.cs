using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.FeatureFlags;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Infrastructure.Repositories;
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
		private readonly IBadgeSettingProvider _agentBadgeSettingsProvider;
		private readonly IPersonNameProvider _personNameProvider;
		private readonly ISiteRepository _siteRepository;
		private readonly ITeamRepository _teamRepository;
		private readonly IToggleManager _toggleManager;
		private readonly IGroupingReadOnlyRepository _groupingRepository;
		private bool toggleEnabled;

		private static readonly Guid PageMain = new Guid("6CE00B41-0722-4B36-91DD-0A3B63C545CF");

		private ReadOnlyGroupDetail[] permittedPersonList;

		public LeaderboardAgentBadgeProvider(IAgentBadgeRepository agentBadgeRepository,
			IAgentBadgeWithRankRepository agentBadgeWithRankRepository,
			IPermissionProvider permissionProvider, IBadgeSettingProvider agentBadgeSettingsProvider,
			IPersonNameProvider personNameProvider,
			ISiteRepository siteRepository, ITeamRepository teamRepository,
			IToggleManager toggleManager, IGroupingReadOnlyRepository groupingRepository)
		{
			_agentBadgeRepository = agentBadgeRepository;
			_agentBadgeWithRankRepository = agentBadgeWithRankRepository;
			_permissionProvider = permissionProvider;
			_agentBadgeSettingsProvider = agentBadgeSettingsProvider;
			_personNameProvider = personNameProvider;
			_siteRepository = siteRepository;
			_teamRepository = teamRepository;
			_toggleManager = toggleManager;
			_groupingRepository = groupingRepository;
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
					var detailsForPage = _groupingRepository.AvailableGroups(new ReadOnlyGroupPage {PageId = PageMain}, queryDate);
					var agentsInSite = new List<ReadOnlyGroupDetail>();
					detailsForPage.ForEach(
						x => agentsInSite.AddRange(_groupingRepository.DetailsForGroup(x.GroupId, queryDate)));
					permittedAgentBadgeList = getPermittedAgentBadgeList(functionPath, agentsInSite, queryDate, setting);
					break;
				}
				case LeadboardQueryType.Site:
				{
					var site = _siteRepository.Get(query.SelectedId);
					var teamsInSite = site.TeamCollection;
					var agentsInSite = new List<ReadOnlyGroupDetail>();
					teamsInSite.ForEach(
						x => agentsInSite.AddRange(_groupingRepository.DetailsForGroup(x.Id.GetValueOrDefault(), queryDate)));

					permittedAgentBadgeList = getPermittedAgentBadgeList(functionPath, agentsInSite, queryDate, setting);
					break;
				}
				case LeadboardQueryType.Team:
				{
					var team = _teamRepository.Get(query.SelectedId);
					var agentsInTeam = _groupingRepository.DetailsForGroup(team.Id.GetValueOrDefault(), queryDate);

					permittedAgentBadgeList = getPermittedAgentBadgeList(functionPath, agentsInTeam, queryDate, setting);
					break;
				}
			}

			return getPermittedAgentOverviews(permittedAgentBadgeList);
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
		private IEnumerable<AgentWithBadge> getPermittedAgentBadgeList(string functionPath, IEnumerable<ReadOnlyGroupDetail> allAgents,
			DateOnly date, IAgentBadgeSettings setting)
		{
			permittedPersonList = getPermittedAgents(functionPath, allAgents, date);
			var permittedPersonIdList = permittedPersonList.Select(x => x.PersonId).ToList();

			var agentBadges = _agentBadgeRepository.Find(permittedPersonIdList);
			var agentWithBadges = getAgentWithBadges(agentBadges, setting.SilverToBronzeBadgeRate, setting.GoldToSilverBadgeRate);
			
			if (toggleEnabled)
			{				
				var agentBadgesWithRank = _agentBadgeWithRankRepository.Find(permittedPersonIdList);
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
				if (!dic.TryGetValue(personId, out overview))
				{
					var detail = permittedPersons.First(p => p.PersonId == personId);
					overview = new AgentBadgeOverview
					{
						AgentName = _personNameProvider.BuildNameFromSetting(detail.FirstName, detail.LastName)
					};
					dic.Add(personId, overview);
				}
				
				overview.Gold += agentBadge.GoldBadgeAmount;
				overview.Silver += agentBadge.SilverBadgeAmount;
				overview.Bronze += agentBadge.BronzeBadgeAmount;
			}
			return dic.Values;
		}

		private ReadOnlyGroupDetail[] getPermittedAgents(string functionPath, IEnumerable<ReadOnlyGroupDetail> personList, DateOnly date)
		{
			return (from t in personList
				where _permissionProvider.HasOrganisationDetailPermission(functionPath, date, t)
				select t).ToArray();
		}
	}
}
