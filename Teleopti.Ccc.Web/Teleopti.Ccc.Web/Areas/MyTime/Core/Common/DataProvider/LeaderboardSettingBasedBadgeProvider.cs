using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.GroupPageCreator;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Web.Areas.MyTime.Models.BadgeLeaderBoardReport;
using Teleopti.Ccc.Web.Core;
using Teleopti.Interfaces.Domain;
using Teleopti.Ccc.Web.Areas.MyTime.Core.Settings.DataProvider;
using Teleopti.Ccc.Domain.SystemSetting.GlobalSetting;

namespace Teleopti.Ccc.Web.Areas.MyTime.Core.Common.DataProvider
{
	public class LeaderboardSettingBasedBadgeProvider : ILeaderboardSettingBasedBadgeProvider
	{
		private class agentWithBadge
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
		private readonly IGroupingReadOnlyRepository _groupingRepository;
		private readonly ISettingsPersisterAndProvider<NameFormatSettings> _nameFormatSettings;

		private ReadOnlyGroupDetail[] permittedPersonList;
		private readonly ITeamGamificationSettingRepository _teamSettingRepository;
		private readonly IPersonRepository _personRepo;
		private IEnumerable<ITeamGamificationSetting> teamSettings;
		private readonly IAgentBadgeTransactionRepository _agentBadgeTransactionRepository;
		private readonly IAgentBadgeWithRankTransactionRepository _agentBadgeWithRankTransactionRepository;

		public LeaderboardSettingBasedBadgeProvider(IAgentBadgeRepository agentBadgeRepository,
			IAgentBadgeWithRankRepository agentBadgeWithRankRepository,
			IPermissionProvider permissionProvider,
			IPersonNameProvider personNameProvider,
			ISiteRepository siteRepository, ITeamRepository teamRepository,
			IGroupingReadOnlyRepository groupingRepository,
			ITeamGamificationSettingRepository teamSettingRepository, IPersonRepository personRepo, 
			ISettingsPersisterAndProvider<NameFormatSettings> nameFormatSettings, IAgentBadgeTransactionRepository agentBadgeTransactionRepository, IAgentBadgeWithRankTransactionRepository agentBadgeWithRankTransactionRepository)
		{
			_agentBadgeRepository = agentBadgeRepository;
			_agentBadgeWithRankRepository = agentBadgeWithRankRepository;
			_permissionProvider = permissionProvider;
			_personNameProvider = personNameProvider;
			_siteRepository = siteRepository;
			_teamRepository = teamRepository;
			_groupingRepository = groupingRepository;
			_teamSettingRepository = teamSettingRepository;
			_personRepo = personRepo;
			_nameFormatSettings = nameFormatSettings;
			_agentBadgeTransactionRepository = agentBadgeTransactionRepository;
			_agentBadgeWithRankTransactionRepository = agentBadgeWithRankTransactionRepository;
		}

		public IEnumerable<AgentBadgeOverview> PermittedAgentBadgeOverviewsForEveryoneOrMyOwn(string functionPath,
			LeaderboardQuery query, DateOnlyPeriod? period = null)
		{
			teamSettings = _teamSettingRepository.FindAllTeamGamificationSettingsSortedByTeam();
			var queryDate = query.Date;

			var detailsForPage = _groupingRepository.AvailableGroups(new ReadOnlyGroupPage {PageId = Group.PageMainId}, queryDate);
			var agentsInSite = new List<ReadOnlyGroupDetail>();
			detailsForPage.ForEach(
				x => agentsInSite.AddRange(_groupingRepository.DetailsForGroup(x.GroupId, queryDate)));

			var permittedAgentBadgeList = getPermittedAgentBadgeListBasedOnSpecificSetting(functionPath, agentsInSite, queryDate,period);

			return getPermittedAgentOverviews(permittedAgentBadgeList);
		}

		public IEnumerable<AgentBadgeOverview> PermittedAgentBadgeOverviewsForSite(string functionPath, LeaderboardQuery query, DateOnlyPeriod? period = null)
		{
			teamSettings = _teamSettingRepository.FindAllTeamGamificationSettingsSortedByTeam();
			var queryDate = query.Date;

			var site = _siteRepository.Get(query.SelectedId);
			var teamsInSite = site.TeamCollection;
			var agentsInSite = new List<ReadOnlyGroupDetail>();
			teamsInSite.ForEach(
				x => agentsInSite.AddRange(_groupingRepository.DetailsForGroup(x.Id.GetValueOrDefault(), queryDate)));

			var permittedAgentBadgeList = getPermittedAgentBadgeListBasedOnSpecificSetting(functionPath, agentsInSite, queryDate,period);

			return getPermittedAgentOverviews(permittedAgentBadgeList);
		}

		public IEnumerable<AgentBadgeOverview> PermittedAgentBadgeOverviewsForTeam(string functionPath, LeaderboardQuery query, DateOnlyPeriod? period = null)
		{
			teamSettings = _teamSettingRepository.FindAllTeamGamificationSettingsSortedByTeam();
			var queryDate = query.Date;

			var team = _teamRepository.Get(query.SelectedId);
			var agentsInTeam = _groupingRepository.DetailsForGroup(team.Id.GetValueOrDefault(), queryDate);

			var permittedAgentBadgeList = getPermittedAgentBadgeListBasedOnSpecificSetting(functionPath, agentsInTeam, queryDate, period);

			return getPermittedAgentOverviews(permittedAgentBadgeList);
		}

		public IEnumerable<AgentBadgeOverview> GetAgentBadgeOverviewsForPeople(IEnumerable<Guid> personIds, DateOnly date, DateOnlyPeriod? period = null)
		{
			teamSettings = _teamSettingRepository.FindAllTeamGamificationSettingsSortedByTeam();
			var agentBadgeList = getAgentBadgeListForPeople(date, personIds.ToList(),period);
			var people = _personRepo.FindPeople(personIds);

			return getPermittedAgentOverviews(agentBadgeList, people);
		} 
		private IEnumerable<agentWithBadge> mergeAgentWithBadges(IEnumerable<agentWithBadge> agentWithBadges1,
			IEnumerable<agentWithBadge> agentWithBadges2)
		{
			var list1 = agentWithBadges1.ToList();
			var list2 = agentWithBadges2.ToList();
			var result = list1.Concat(list2)
				.GroupBy(badge => badge.Person)
				.Select(group => new agentWithBadge
				{
					Person = group.Key,
					BronzeBadgeAmount = group.Sum(x => x.BronzeBadgeAmount),
					SilverBadgeAmount = group.Sum(x => x.SilverBadgeAmount),
					GoldBadgeAmount = group.Sum(x => x.GoldBadgeAmount)
				});
			return result;
		}

		private IEnumerable<agentWithBadge> getPermittedAgentBadgeListBasedOnSpecificSetting(string functionPath,
			IEnumerable<ReadOnlyGroupDetail> allAgents,
			DateOnly date,
			DateOnlyPeriod? period)
		{
			permittedPersonList = getPermittedAgents(functionPath, allAgents, date);
			var permittedPersonIdList = permittedPersonList.Select(x => x.PersonId).ToList();

			return getAgentBadgeListForPeople(date, permittedPersonIdList, period);
		}

		private IEnumerable<agentWithBadge> getAgentBadgeListForPeople(DateOnly date, IList<Guid> permittedPersonIdList, DateOnlyPeriod? period)
		{
			var agentWithBadgesConvertedFromRatio = new List<agentWithBadge>();
			var agentListWithDifferentThreshold = new List<IPerson>();			

			foreach (var teamSetting in teamSettings)
			{
				var agentsInTeam = _personRepo.FindPeopleBelongTeam(teamSetting.Team, new DateOnlyPeriod(date.AddDays(-1), date));
				
				var agentsInTeamWithPermission = agentsInTeam.Where(p => permittedPersonIdList.Contains(p.Id.Value)).ToList();
	
				ICollection<AgentBadge> agentBadgesConvertedFromRatio;
				if (period.HasValue)
				{					
					agentBadgesConvertedFromRatio = AgentBadge.FromAgentBadgeTransaction(_agentBadgeTransactionRepository.Find(agentsInTeamWithPermission, period.Value));
				}
				else
				{
					var agentIdsInTeamWithPermission = agentsInTeamWithPermission.Select(x => x.Id.GetValueOrDefault()).ToList();
					agentBadgesConvertedFromRatio = _agentBadgeRepository.Find(agentIdsInTeamWithPermission);					
				}
				
				agentWithBadgesConvertedFromRatio.AddRange(getAgentWithBadges(agentBadgesConvertedFromRatio,
					teamSetting.GamificationSetting.SilverToBronzeBadgeRate, teamSetting.GamificationSetting.GoldToSilverBadgeRate));

				agentListWithDifferentThreshold.AddRange(agentsInTeamWithPermission);
			}

			IList<IAgentBadgeWithRank> agentBadgesWithRank;
			if (period.HasValue)
			{
				agentBadgesWithRank = AgentBadgeWithRank.FromAgentBadgeWithRanksTransaction(_agentBadgeWithRankTransactionRepository.Find(agentListWithDifferentThreshold, period.Value));
			}
			else
			{
				var agentIdList = agentListWithDifferentThreshold.Select(p => p.Id.GetValueOrDefault()).ToArray();			
				agentBadgesWithRank = _agentBadgeWithRankRepository.Find(agentIdList).ToList();
			}
			var agentWithRankedBadges = getAgentWithBadges(agentBadgesWithRank).ToList();
			return mergeAgentWithBadges(agentWithBadgesConvertedFromRatio, agentWithRankedBadges);
		}

		private static IEnumerable<agentWithBadge> getAgentWithBadges(IEnumerable<AgentBadge> agentBadges,
			int silverToBronzeRate, int goldToSilverRate)
		{

			var result = agentBadges == null
				? new List<agentWithBadge>()
				: agentBadges.GroupBy(x => x.Person)
					.Select(group => new agentWithBadge
					{
						Person = group.Key,
						GoldBadgeAmount = group.Sum(x => x.GetGoldBadge(silverToBronzeRate, goldToSilverRate)),
						SilverBadgeAmount = group.Sum(x => x.GetSilverBadge(silverToBronzeRate, goldToSilverRate)),
						BronzeBadgeAmount = group.Sum(x => x.GetBronzeBadge(silverToBronzeRate, goldToSilverRate))
					});

			return result;
		}

		private static IEnumerable<agentWithBadge> getAgentWithBadges(IEnumerable<IAgentBadgeWithRank> agentBadges)
		{
			var result = agentBadges == null
				? new List<agentWithBadge>()
				: agentBadges.GroupBy(x => x.Person)
					.Select(group => new agentWithBadge
					{
						Person = group.Key,
						GoldBadgeAmount = group.Sum(x => x.GoldBadgeAmount),
						SilverBadgeAmount = group.Sum(x => x.SilverBadgeAmount),
						BronzeBadgeAmount = group.Sum(x => x.BronzeBadgeAmount)
					});

			return result;
		}

		private IEnumerable<AgentBadgeOverview> getPermittedAgentOverviews(IEnumerable<agentWithBadge> permittedAgentBadgeList)
		{
			var permittedPersons = permittedPersonList.ToList();
			var dic = new Dictionary<Guid, AgentBadgeOverview>();
			var nameSetting = _nameFormatSettings.Get();

			foreach (var agentBadge in permittedAgentBadgeList)
			{
				var personId = agentBadge.Person;

				AgentBadgeOverview overview;
				if (!dic.TryGetValue(personId, out overview))
				{
					var detail = permittedPersons.First(p => p.PersonId == personId);
					overview = new AgentBadgeOverview
					{
						AgentName = _personNameProvider.BuildNameFromSetting(detail.FirstName, detail.LastName, nameSetting)
					};
					dic.Add(personId, overview);
				}

				overview.Gold += agentBadge.GoldBadgeAmount;
				overview.Silver += agentBadge.SilverBadgeAmount;
				overview.Bronze += agentBadge.BronzeBadgeAmount;
			}
			return dic.Values;
		}
		private IEnumerable<AgentBadgeOverview> getPermittedAgentOverviews(IEnumerable<agentWithBadge> permittedAgentBadgeList, IEnumerable<IPerson> permittedPeople )
		{
			var dic = new Dictionary<Guid, AgentBadgeOverview>();
			var nameSetting = _nameFormatSettings.Get();

			foreach (var agentBadge in permittedAgentBadgeList)
			{
				var personId = agentBadge.Person;

				AgentBadgeOverview overview;
				if (!dic.TryGetValue(personId, out overview))
				{
					var person = permittedPeople.First(p => p.Id.GetValueOrDefault() == personId);
					overview = new AgentBadgeOverview
					{
						AgentName = _personNameProvider.BuildNameFromSetting(person.Name.FirstName, person.Name.LastName, nameSetting)
					};
					dic.Add(personId, overview);
				}

				overview.Gold += agentBadge.GoldBadgeAmount;
				overview.Silver += agentBadge.SilverBadgeAmount;
				overview.Bronze += agentBadge.BronzeBadgeAmount;
			}
			return dic.Values;
		}

		private ReadOnlyGroupDetail[] getPermittedAgents(string functionPath, IEnumerable<ReadOnlyGroupDetail> personList,
			DateOnly date)
		{
			return (from t in personList
				where _permissionProvider.HasOrganisationDetailPermission(functionPath, date, t)
				select t).ToArray();
		}
	}
}
