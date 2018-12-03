using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.UserTexts;
using Teleopti.Ccc.Web.Areas.MyTime.Models.Portal;


namespace Teleopti.Ccc.Web.Areas.MyTime.Core.Common.DataProvider
{
	public interface IAgentBadgeWithinPeriodProvider
	{
		IEnumerable<BadgeViewModel> GetBadges(DateOnlyPeriod period);
	}

	public class AgentBadgeWithinPeriodProvider : IAgentBadgeWithinPeriodProvider
	{
		private readonly ILoggedOnUser _loggedOnUser;
		private readonly ITeamGamificationSettingRepository _teamGamificationSettingRepo;
		private readonly IAgentBadgeWithRankTransactionRepository _agentBadgeWithRankTransactionRepository;
		private readonly IAgentBadgeTransactionRepository _agentBadgeTransactionRepository;

		public AgentBadgeWithinPeriodProvider(ILoggedOnUser loggedOnUser, ITeamGamificationSettingRepository teamGamificationSettingRepo, IAgentBadgeWithRankTransactionRepository agentBadgeWithRankTransactionRepository, IAgentBadgeTransactionRepository agentBadgeTransactionRepository)
		{
			_loggedOnUser = loggedOnUser;
			_teamGamificationSettingRepo = teamGamificationSettingRepo;
			_agentBadgeWithRankTransactionRepository = agentBadgeWithRankTransactionRepository;
			_agentBadgeTransactionRepository = agentBadgeTransactionRepository;
		}

		public IEnumerable<BadgeViewModel> GetBadges(DateOnlyPeriod period)
		{
			var currentUser = _loggedOnUser.CurrentUser();
			if (currentUser == null) return new List<BadgeViewModel>();

			var teamGamificationSetting = _teamGamificationSettingRepo.FindTeamGamificationSettingsByTeam(currentUser.MyTeam(DateOnly.Today));
			if (teamGamificationSetting == null) return new List<BadgeViewModel>();

			var enabledBadgeTypes = teamGamificationSetting.GamificationSetting.EnabledBadgeTypes();

			var agentBadgeWithRank = getAgentBadgeWithRank(currentUser, enabledBadgeTypes, period, teamGamificationSetting);
			var agentBadge = getAgentBadge(currentUser, enabledBadgeTypes, period, teamGamificationSetting);

			return mergeBadges(agentBadgeWithRank, agentBadge);
		}

		private IList<BadgeViewModel> getAgentBadge(IPerson currentUser, IEnumerable<BadgeTypeInfo> enabledBadgeTypes, DateOnlyPeriod period, ITeamGamificationSetting teamGamificationSetting)
		{
			var setting = teamGamificationSetting.GamificationSetting;
			var silverToBronzeBadgeRate = setting.SilverToBronzeBadgeRate;
			var goldToSilverBadgeRate = setting.GoldToSilverBadgeRate;

			return (from badgeType in enabledBadgeTypes
				let agentBadge = _agentBadgeTransactionRepository.Find(currentUser, badgeType.Id, period, badgeType.IsExternal)
				select new BadgeViewModel
				{
					BadgeType = badgeType.Id,
					GoldBadge = AgentBadge.getGoldBadgeCount(agentBadge.Sum(ab => ab.Amount), silverToBronzeBadgeRate, goldToSilverBadgeRate),
					SilverBadge = AgentBadge.getSilverBadgeCount(agentBadge.Sum(ab => ab.Amount), silverToBronzeBadgeRate, goldToSilverBadgeRate),
					BronzeBadge = AgentBadge.getBronzeBadgeCount(agentBadge.Sum(ab => ab.Amount), silverToBronzeBadgeRate),
					IsExternal = badgeType.IsExternal,
					Name = findBadgeTypeName(badgeType.Id, badgeType.IsExternal, teamGamificationSetting)
				}).ToList();
		}

		private IList<BadgeViewModel> getAgentBadgeWithRank(IPerson currentUser, IEnumerable<BadgeTypeInfo> enabledBadgeTypes, DateOnlyPeriod period, ITeamGamificationSetting teamGamificationSetting)
		{
			return (from badgeType in enabledBadgeTypes
				let agentBadge = _agentBadgeWithRankTransactionRepository.Find(currentUser, badgeType.Id, period, badgeType.IsExternal)
				select new BadgeViewModel
				{
					BadgeType = badgeType.Id,
					BronzeBadge = agentBadge.Sum(ab => ab.BronzeBadgeAmount),
					SilverBadge = agentBadge.Sum(ab => ab.SilverBadgeAmount),
					GoldBadge = agentBadge.Sum(ab => ab.GoldBadgeAmount),
					IsExternal = badgeType.IsExternal,
					Name = findBadgeTypeName(badgeType.Id, badgeType.IsExternal, teamGamificationSetting)
				}).ToList();
		}

		private IList<BadgeViewModel> mergeBadges(IEnumerable<BadgeViewModel> badgeVmList1,
			IEnumerable<BadgeViewModel> badgeVmList2)
		{
			var totalBadgeVm = badgeVmList1.Concat(badgeVmList2)
				.GroupBy(x => new { x.IsExternal, x.BadgeType })
				.Select(group => new BadgeViewModel
				{
					BadgeType = group.Key.BadgeType,
					IsExternal = group.Key.IsExternal,
					Name = group.First().Name,
					BronzeBadge = group.Sum(x => x.BronzeBadge),
					SilverBadge = group.Sum(x => x.SilverBadge),
					GoldBadge = group.Sum(x => x.GoldBadge)
				})
				.ToList();

			return totalBadgeVm;
		}

		private string findBadgeTypeName(int badgeType, bool isExternal, ITeamGamificationSetting teamGamificationSetting)
		{
			if (isExternal)
			{
				return teamGamificationSetting.GamificationSetting.GetExternalBadgeTypeName(badgeType);
			}

			switch (badgeType)
			{
				case BadgeType.AnsweredCalls:
					return Resources.AnsweredCalls;
				case BadgeType.Adherence:
					return Resources.ReadyTimeAdherence;
				case BadgeType.AverageHandlingTime:
					return Resources.AverageHandlingTime;
				default:
					return "";
			}
		}
	}
}