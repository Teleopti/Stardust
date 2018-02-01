using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.UserTexts;
using Teleopti.Ccc.Web.Areas.MyTime.Models.Portal;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Web.Areas.MyTime.Core.Portal.DataProvider
{
	public class BadgeProvider : IBadgeProvider
	{
		private readonly ILoggedOnUser _loggedOnUser;
		private readonly IAgentBadgeRepository _badgeRepository;
		private readonly IAgentBadgeWithRankRepository _badgeWithRankRepository;
		private ITeamGamificationSetting _teamGamificationSetting;
		private readonly ITeamGamificationSettingRepository _teamGamificationSettingRepo;

		public BadgeProvider(ILoggedOnUser loggedOnUser, IAgentBadgeRepository badgeRepository,
			IAgentBadgeWithRankRepository badgeWithRankRepository,
			ITeamGamificationSettingRepository teamGamificationSettingRepo)
		{
			_loggedOnUser = loggedOnUser;
			_badgeRepository = badgeRepository;
			_badgeWithRankRepository = badgeWithRankRepository;
			_teamGamificationSettingRepo = teamGamificationSettingRepo;
		}

		public IEnumerable<BadgeViewModel> GetBadges()
		{
			var currentUser = _loggedOnUser.CurrentUser();
			if (currentUser == null) return new List<BadgeViewModel>();

			_teamGamificationSetting =
				_teamGamificationSettingRepo.FindTeamGamificationSettingsByTeam(currentUser.MyTeam(DateOnly.Today));

			var enabledBadgeTypes = _teamGamificationSetting.GamificationSetting.EnabledBadgeTypes();

			var allBadges = getBadgesWithoutRank(currentUser, enabledBadgeTypes);
			var allBadgesWithRank = getBadgesWithRank(currentUser, enabledBadgeTypes);

			return mergeBadges(allBadges, allBadgesWithRank);
		}

		private IList<BadgeViewModel> getBadgesWithRank(IPerson p, IEnumerable<BadgeTypeInfo> badgeTypes)
		{
			if (_teamGamificationSetting == null || p == null)
				return new List<BadgeViewModel>();

			return badgeTypes
				.Select(bt => findBadgesWithRank(p, bt.Id, bt.IsExternal) ?? new AgentBadgeWithRank
				{
					BadgeType = bt.Id,
					IsExternal = bt.IsExternal,
					GoldBadgeAmount = 0,
					SilverBadgeAmount = 0,
					BronzeBadgeAmount = 0
				})
				.Select(b => new BadgeViewModel
				{
					BadgeType = b.BadgeType,
					IsExternal = b.IsExternal,
					Name = findBadgeTypeName(b.BadgeType, b.IsExternal),
					GoldBadge = b.GoldBadgeAmount,
					SilverBadge = b.SilverBadgeAmount,
					BronzeBadge = b.BronzeBadgeAmount
				})
				.ToList();
		}

		private IList<BadgeViewModel> getBadgesWithoutRank(IPerson p, IEnumerable<BadgeTypeInfo> badgeTypes)
		{
			if (_teamGamificationSetting == null || p == null)
				return new List<BadgeViewModel>();

			var setting = _teamGamificationSetting.GamificationSetting;
			var silverToBronzeBadgeRate = setting.SilverToBronzeBadgeRate;
			var goldToSilverBadgeRate = setting.GoldToSilverBadgeRate;

			return badgeTypes
				.Select(bt => findBadgesWithoutRank(p, bt.Id, bt.IsExternal) ?? new AgentBadge
				{
					BadgeType = bt.Id,
					IsExternal = bt.IsExternal,
					TotalAmount = 0
				})
				.Select(b => new BadgeViewModel
				{
					BadgeType = b.BadgeType,
					IsExternal = b.IsExternal,
					Name = findBadgeTypeName(b.BadgeType, b.IsExternal),
					GoldBadge = b.GetGoldBadge(silverToBronzeBadgeRate, goldToSilverBadgeRate),
					SilverBadge = b.GetSilverBadge(silverToBronzeBadgeRate, goldToSilverBadgeRate),
					BronzeBadge = b.GetBronzeBadge(silverToBronzeBadgeRate, goldToSilverBadgeRate)
				})
				.ToList();
		}

		private IAgentBadgeWithRank findBadgesWithRank(IPerson p, int badgeType, bool isExternal) => _badgeWithRankRepository.Find(p, badgeType, isExternal);
		private AgentBadge findBadgesWithoutRank(IPerson p, int badgeType, bool isExternal) => _badgeRepository.Find(p, badgeType, isExternal);

		private string findBadgeTypeName(int badgeType, bool isExternal)
		{
			string name = "";
			if (isExternal)
			{
				name = _teamGamificationSetting.GamificationSetting.GetExternalBadgeTypeName(badgeType);
				return name;
			}
			switch (badgeType)
			{
				case BadgeType.AnsweredCalls:
					name = Resources.AnsweredCalls;
					break;
				case BadgeType.Adherence:
					name = Resources.Adherence;
					break;
				case BadgeType.AverageHandlingTime:
					name = Resources.AverageHandlingTime;
					break;
				default:
					break;
			}
			return name;
		}

		/// <summary>
		/// Merge total amount of badges.
		/// To handle the scenario that switch from old badge calculation
		/// </summary>
		/// <param name="badgeVmList1"></param>
		/// <param name="badgeVmList2"></param>
		/// <returns></returns>
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
	}
}