using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.AgentInfo;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.FeatureFlags;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Infrastructure.Toggle;
using Teleopti.Ccc.Web.Areas.MyTime.Models.Portal;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Web.Areas.MyTime.Core.Portal.DataProvider
{
	public class BadgeProvider : IBadgeProvider
	{
		private readonly ILoggedOnUser _loggedOnUser;
		private readonly IAgentBadgeRepository _badgeRepository;
		private readonly IAgentBadgeWithRankRepository _badgeWithRankRepository;
		private readonly IAgentBadgeSettingsRepository _settingsRepository;
		private readonly IToggleManager _toggleManager;
		private IAgentBadgeSettings _settings;
		private ITeamGamificationSetting _teamGamificationSetting;
		private readonly ITeamGamificationSettingRepository _teamGamificationSettingRepo;

		public BadgeProvider(ILoggedOnUser loggedOnUser, IAgentBadgeRepository badgeRepository,
			IAgentBadgeWithRankRepository badgeWithRankRepository, IAgentBadgeSettingsRepository settingsRepository,
			ITeamGamificationSettingRepository teamGamificationSettingRepo,
			IToggleManager toggleManager)
		{
			_loggedOnUser = loggedOnUser;
			_badgeRepository = badgeRepository;
			_badgeWithRankRepository = badgeWithRankRepository;
			_settingsRepository = settingsRepository;
			_toggleManager = toggleManager;
			_teamGamificationSettingRepo = teamGamificationSettingRepo;
		}

		public IEnumerable<BadgeViewModel> GetBadges()
		{	
			var badgeVmList = new List<BadgeViewModel>();
			var currentUser = _loggedOnUser.CurrentUser();
			if (currentUser == null) return badgeVmList;
			_settings = _settingsRepository.GetSettings();
			_teamGamificationSetting =
				_teamGamificationSettingRepo.FindTeamGamificationSettingsByTeam(currentUser.MyTeam(DateOnly.Today));
			var toggleEnabled = _toggleManager.IsEnabled(Toggles.Gamification_NewBadgeCalculation_31185);

			var allBadges = getBadgesWithoutRank(currentUser);
			var allBadgesWithRank = (toggleEnabled)
				? getBadgesWithRank(currentUser)
				: new List<BadgeViewModel>();

			return mergeBadges(allBadges, allBadgesWithRank);
		}

		private IEnumerable<BadgeViewModel> getBadgesWithRank(IPerson person)
		{
			if (person == null)
			{
				return new List<BadgeViewModel>();
			}

			var badges = new List<IAgentBadgeWithRank>();
			if (_toggleManager.IsEnabled(Toggles.Portal_DifferentiateBadgeSettingForAgents_31318))
			{
				IGamificationSetting setting = _teamGamificationSetting.GamificationSetting;
				if (setting.AdherenceBadgeEnabled)
				{
					retrieveBadgeWithRank(person, BadgeType.Adherence, badges);
				}

				if (setting.AHTBadgeEnabled)
				{
					retrieveBadgeWithRank(person, BadgeType.AverageHandlingTime, badges);
				}

				if (setting.AnsweredCallsBadgeEnabled)
				{
					retrieveBadgeWithRank(person, BadgeType.AnsweredCalls, badges);
				}
			}
			else
			{
				if (_settings.AdherenceBadgeEnabled)
				{
					retrieveBadgeWithRank(person, BadgeType.Adherence, badges);
				}

				if (_settings.AHTBadgeEnabled)
				{
					retrieveBadgeWithRank(person, BadgeType.AverageHandlingTime, badges);
				}

				if (_settings.AnsweredCallsBadgeEnabled)
				{
					retrieveBadgeWithRank(person, BadgeType.AnsweredCalls, badges);
				}
			}
			

			var badgeVmList = badges.Select(x => new BadgeViewModel
			{
				BadgeType = x.BadgeType,
				BronzeBadge = x.BronzeBadgeAmount,
				SilverBadge = x.SilverBadgeAmount,
				GoldBadge = x.GoldBadgeAmount
			});
			return badgeVmList;
		}

		private IEnumerable<BadgeViewModel> getBadgesWithoutRank(IPerson person)
		{
			if (person == null)
			{
				return new List<BadgeViewModel>();
			}
			var badges = new List<IAgentBadge>();
			int silverToBronzeBadgeRate;
			int goldToSilverBadgeRate;

			if (_toggleManager.IsEnabled(Toggles.Portal_DifferentiateBadgeSettingForAgents_31318))
			{
				IGamificationSetting setting = _teamGamificationSetting.GamificationSetting;

				if (setting.AdherenceBadgeEnabled)
				{
					retrieveBadgeWithoutRank(person, BadgeType.Adherence, badges);
				}

				if (setting.AHTBadgeEnabled)
				{
					retrieveBadgeWithoutRank(person, BadgeType.AverageHandlingTime, badges);
				}

				if (setting.AnsweredCallsBadgeEnabled)
				{
					retrieveBadgeWithoutRank(person, BadgeType.AnsweredCalls, badges);
				}
				silverToBronzeBadgeRate = setting.GamificationSettingRuleSet == GamificationSettingRuleSet.RuleWithRatioConvertor
					? setting.SilverToBronzeBadgeRate
					: new GamificationSetting(" ").SilverToBronzeBadgeRate;
				goldToSilverBadgeRate = setting.GamificationSettingRuleSet == GamificationSettingRuleSet.RuleWithRatioConvertor
					? setting.GoldToSilverBadgeRate
					: new GamificationSetting(" ").GoldToSilverBadgeRate;
			}
			else
			{
				if (_settings.AdherenceBadgeEnabled)
				{
					retrieveBadgeWithoutRank(person, BadgeType.Adherence, badges);
				}

				if (_settings.AHTBadgeEnabled)
				{
					retrieveBadgeWithoutRank(person, BadgeType.AverageHandlingTime, badges);
				}

				if (_settings.AnsweredCallsBadgeEnabled)
				{
					retrieveBadgeWithoutRank(person, BadgeType.AnsweredCalls, badges);
				}
				silverToBronzeBadgeRate = _settings.SilverToBronzeBadgeRate;
				goldToSilverBadgeRate = _settings.GoldToSilverBadgeRate;
			}
			

			var badgeVmList = badges.Select(x => new BadgeViewModel
			{
				BadgeType = x.BadgeType,
				BronzeBadge = x.GetBronzeBadge(silverToBronzeBadgeRate, goldToSilverBadgeRate),
				SilverBadge = x.GetSilverBadge(silverToBronzeBadgeRate, goldToSilverBadgeRate),
				GoldBadge = x.GetGoldBadge(silverToBronzeBadgeRate, goldToSilverBadgeRate)
			});
			return badgeVmList;
		}

		private void retrieveBadgeWithoutRank(IPerson person, BadgeType badgeType, ICollection<IAgentBadge> badges)
		{
			var badge = _badgeRepository.Find(person, badgeType);
			if (badge != null)
				badges.Add(badge);
		}

		private void retrieveBadgeWithRank(IPerson person, BadgeType badgeType, ICollection<IAgentBadgeWithRank> badges)
		{
			var badge = _badgeWithRankRepository.Find(person, badgeType);
			if (badge != null)
				badges.Add(badge);
		}

		/// <summary>
		/// Merge total amount of badges.
		/// To handle the scenario that switch from old badge calculation 
		/// </summary>
		/// <param name="badgeVmList1"></param>
		/// <param name="badgeVmList2"></param>
		/// <returns></returns>
		private IEnumerable<BadgeViewModel> mergeBadges(IEnumerable<BadgeViewModel> badgeVmList1,
			IEnumerable<BadgeViewModel> badgeVmList2)
		{
			var totalBadgeVm = badgeVmList1.Concat(badgeVmList2)
				.GroupBy(x => x.BadgeType)
				.Select(group => new BadgeViewModel
				{
					BadgeType = group.Key,
					BronzeBadge = group.Sum(x => x.BronzeBadge),
					SilverBadge = group.Sum(x => x.SilverBadge),
					GoldBadge = group.Sum(x => x.GoldBadge)
				});

			return totalBadgeVm;
		}
	}
}