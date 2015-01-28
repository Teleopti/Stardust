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
	public class LeaderboardSettingBasedBadgeProvider : ILeaderboardSettingBasedBadgeProvider
	{
		private class AgentWithBadge
		{
			public Guid Person { get; set; }
			public int BronzeBadgeAmount { get; set; }
			public int SilverBadgeAmount { get; set; }
			public int GoldBadgeAmount { get; set; }
		}

		private readonly IAgentBadgeRepository _agentBadgeRepository;
		private readonly IAgentBadgeWithRankRepository _agentBadgeWithRankRepository;
		private readonly IPermissionProvider _permissionProvider;
		private readonly IPersonNameProvider _personNameProvider;
		private readonly ISiteRepository _siteRepository;
		private readonly ITeamRepository _teamRepository;
		private readonly IToggleManager _toggleManager;
		private readonly IGroupingReadOnlyRepository _groupingRepository;
		private bool toggleEnabled;

		private static readonly Guid PageMain = new Guid("6CE00B41-0722-4B36-91DD-0A3B63C545CF");

		private ReadOnlyGroupDetail[] permittedPersonList;
		private ITeamGamificationSettingRepository _teamSettingRepository;
		private IPersonRepository _personRepo;
		private IEnumerable<ITeamGamificationSetting> teamSettings;

		public LeaderboardSettingBasedBadgeProvider(IAgentBadgeRepository agentBadgeRepository,
			IAgentBadgeWithRankRepository agentBadgeWithRankRepository,
			IPermissionProvider permissionProvider,
			IPersonNameProvider personNameProvider,
			ISiteRepository siteRepository, ITeamRepository teamRepository,
			IToggleManager toggleManager, IGroupingReadOnlyRepository groupingRepository, ITeamGamificationSettingRepository teamSettingRepository, IPersonRepository personRepo)
		{
			_agentBadgeRepository = agentBadgeRepository;
			_agentBadgeWithRankRepository = agentBadgeWithRankRepository;
			_permissionProvider = permissionProvider;
			_personNameProvider = personNameProvider;
			_siteRepository = siteRepository;
			_teamRepository = teamRepository;
			_toggleManager = toggleManager;
			_groupingRepository = groupingRepository;
			_teamSettingRepository = teamSettingRepository;
			_personRepo = personRepo;
			
			toggleEnabled = _toggleManager.IsEnabled(Toggles.Gamification_NewBadgeCalculation_31185);
		}

		public IEnumerable<AgentBadgeOverview> PermittedAgentBadgeOverviewsForEveryoneOrMyOwn(string functionPath, LeaderboardQuery query)
		{
			teamSettings = _teamSettingRepository.FindAllTeamGamificationSettingsSortedByTeam();
			toggleEnabled = _toggleManager.IsEnabled(Toggles.Gamification_NewBadgeCalculation_31185);
			var queryDate = query.Date;
			
			var detailsForPage = _groupingRepository.AvailableGroups(new ReadOnlyGroupPage { PageId = PageMain }, queryDate);
			var agentsInSite = new List<ReadOnlyGroupDetail>();
			detailsForPage.ForEach(
				x => agentsInSite.AddRange(_groupingRepository.DetailsForGroup(x.GroupId, queryDate)));

			var permittedAgentBadgeList = getPermittedAgentBadgeListBasedOnSpecificSetting(functionPath, agentsInSite, queryDate);

			return getPermittedAgentOverviews(permittedAgentBadgeList);
		}

		public IEnumerable<AgentBadgeOverview> PermittedAgentBadgeOverviewsForSite(string functionPath, LeaderboardQuery query)
		{
			teamSettings = _teamSettingRepository.FindAllTeamGamificationSettingsSortedByTeam();
			toggleEnabled = _toggleManager.IsEnabled(Toggles.Gamification_NewBadgeCalculation_31185);
			var queryDate = query.Date;

			var site = _siteRepository.Get(query.SelectedId);
			var teamsInSite = site.TeamCollection;
			var agentsInSite = new List<ReadOnlyGroupDetail>();
			teamsInSite.ForEach(
				x => agentsInSite.AddRange(_groupingRepository.DetailsForGroup(x.Id.GetValueOrDefault(), queryDate)));

			var permittedAgentBadgeList = getPermittedAgentBadgeListBasedOnSpecificSetting(functionPath, agentsInSite, queryDate);

			return getPermittedAgentOverviews(permittedAgentBadgeList);
		}

		public IEnumerable<AgentBadgeOverview> PermittedAgentBadgeOverviewsForTeam(string functionPath, LeaderboardQuery query)
		{
			teamSettings = _teamSettingRepository.FindAllTeamGamificationSettingsSortedByTeam();
			toggleEnabled = _toggleManager.IsEnabled(Toggles.Gamification_NewBadgeCalculation_31185);
			var queryDate = query.Date;

			var team = _teamRepository.Get(query.SelectedId);
			var agentsInTeam = _groupingRepository.DetailsForGroup(team.Id.GetValueOrDefault(), queryDate);

			var permittedAgentBadgeList = getPermittedAgentBadgeListBasedOnSpecificSetting(functionPath, agentsInTeam, queryDate);

			return getPermittedAgentOverviews(permittedAgentBadgeList);
		}

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

		private IEnumerable<AgentWithBadge> getPermittedAgentBadgeListBasedOnSpecificSetting(string functionPath, IEnumerable<ReadOnlyGroupDetail> allAgents,
			DateOnly date)
		{
			permittedPersonList = getPermittedAgents(functionPath, allAgents, date);
			var permittedPersonIdList = permittedPersonList.Select(x => x.PersonId);

			
			var agentWithBadgesConvertedFromRatio = new List<AgentWithBadge>();
			var agentIdListWithDifferentThreshold = new List<Guid>();
			foreach (var teamSetting in teamSettings)
			{
				var agentsInTeam = _personRepo.FindPeopleBelongTeam(teamSetting.Team, new DateOnlyPeriod(date.AddDays(-1), date));

				var agentIdListWithSetting = agentsInTeam.Select(a => a.Id.GetValueOrDefault());

				var permittedAgentIdListWithSetting = permittedPersonIdList.Intersect(agentIdListWithSetting).ToList();

				var agentBadgesConvertedFromRatio = _agentBadgeRepository.Find(permittedAgentIdListWithSetting);
				agentWithBadgesConvertedFromRatio.AddRange(getAgentWithBadges(agentBadgesConvertedFromRatio,
					teamSetting.GamificationSetting.SilverToBronzeBadgeRate, teamSetting.GamificationSetting.GoldToSilverBadgeRate));
				if (toggleEnabled)
				{
					agentIdListWithDifferentThreshold.AddRange(permittedAgentIdListWithSetting);
				}

			}

			if (toggleEnabled)
			{
				var agentBadgesWithRank = _agentBadgeWithRankRepository.Find(agentIdListWithDifferentThreshold);
				var agentWithRankedBadges = getAgentWithBadges(agentBadgesWithRank).ToList();
				return mergeAgentWithBadges(agentWithBadgesConvertedFromRatio, agentWithRankedBadges);
			}
			return agentWithBadgesConvertedFromRatio;		
		}

		private static IEnumerable<AgentWithBadge> getAgentWithBadges(IEnumerable<IAgentBadge> agentBadges,
			int silverToBronzeRate, int goldToSilverRate)
		{

			var result = agentBadges==null? new List<AgentWithBadge>() : agentBadges.GroupBy(x => x.Person)
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
			var result = agentBadges == null ? new List<AgentWithBadge>() : agentBadges.GroupBy(x => x.Person)
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
