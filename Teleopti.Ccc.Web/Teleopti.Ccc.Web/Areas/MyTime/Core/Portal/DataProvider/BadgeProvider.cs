using System.Collections.Generic;
using System.Linq;
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

		public BadgeProvider(ILoggedOnUser loggedOnUser, IAgentBadgeRepository badgeRepository,
			IAgentBadgeWithRankRepository badgeWithRankRepository, IAgentBadgeSettingsRepository settingsRepository,
			IToggleManager toggleManager)
		{
			_loggedOnUser = loggedOnUser;
			_badgeRepository = badgeRepository;
			_badgeWithRankRepository = badgeWithRankRepository;
			_settingsRepository = settingsRepository;
			_toggleManager = toggleManager;
		}

		public IEnumerable<BadgeViewModel> GetBadges()
		{
			_settings = _settingsRepository.GetSettings();
			var badgeVmList = new List<BadgeViewModel>();
			var currentUser = _loggedOnUser.CurrentUser();
			if (currentUser == null) return badgeVmList;

			var toggleEnabled = _toggleManager.IsEnabled(Toggles.Gamification_NewBadgeCalculation_31185);

			var allBadges = getBadgesWithoutRank(currentUser);
			var allBadgesWithRank = (toggleEnabled && _settings.CalculateBadgeWithRank) ? getBadgesWithRank(currentUser) : null;

			return mergeBadges(allBadges, allBadgesWithRank,
				_settings.SilverToBronzeBadgeRate, _settings.GoldToSilverBadgeRate);
		}

		private IEnumerable<IAgentBadgeWithRank> getBadgesWithRank(IPerson person)
		{
			var badges = new List<IAgentBadgeWithRank>();
			if (person == null) return badges;

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

			return badges;
		}

		private IEnumerable<IAgentBadge> getBadgesWithoutRank(IPerson person)
		{
			var badges = new List<IAgentBadge>();
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
			return badges;
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
		/// <param name="allBadges">Badges calculated with old method</param>
		/// <param name="allBadgesWithRank">Badges calculated with rank</param>
		/// <param name="silverToBronzeBadgeRate"></param>
		/// <param name="goldToSilverBadgeRate"></param>
		/// <returns></returns>
		private IEnumerable<BadgeViewModel> mergeBadges(IEnumerable<IAgentBadge> allBadges,
			IEnumerable<IAgentBadgeWithRank> allBadgesWithRank,
			int silverToBronzeBadgeRate, int goldToSilverBadgeRate)
		{
			var agentWithBadgeList = allBadges == null
				? null
				: allBadges.ToDictionary(x => new {x.Person, x.BadgeType}, x => new
				{
					BronzeBadge = x.GetBronzeBadge(silverToBronzeBadgeRate, goldToSilverBadgeRate),
					SilverBadge = x.GetSilverBadge(silverToBronzeBadgeRate, goldToSilverBadgeRate),
					GoldBadge = x.GetGoldBadge(silverToBronzeBadgeRate, goldToSilverBadgeRate)
				});


			var badgeWithRankList = allBadgesWithRank == null
				? null
				: allBadgesWithRank.ToDictionary(x => new {x.Person, x.BadgeType}, x => new
				{
					BronzeBadge = x.BronzeBadgeAmount,
					SilverBadge = x.SilverBadgeAmount,
					GoldBadge = x.GoldBadgeAmount
				});

			if (agentWithBadgeList == null || !agentWithBadgeList.Any())
			{
				// If there is no badges calculated with old method, then return badges with rank.
				agentWithBadgeList = badgeWithRankList;
			}
			else if (badgeWithRankList != null && badgeWithRankList.Any())
			{
				// Else merge the 2 kind of badges together.
				var personHasBadge = badgeWithRankList.Keys;
				foreach (var personAndBadgeType in personHasBadge)
				{
					var badgeWithRank = badgeWithRankList[personAndBadgeType];
					if (agentWithBadgeList.ContainsKey(personAndBadgeType))
					{
						var agentWithBadgeVm = agentWithBadgeList[personAndBadgeType];
						agentWithBadgeList[personAndBadgeType] = new
						{
							BronzeBadge = agentWithBadgeVm.BronzeBadge + badgeWithRank.BronzeBadge,
							SilverBadge = agentWithBadgeVm.SilverBadge + badgeWithRank.SilverBadge,
							GoldBadge = agentWithBadgeVm.GoldBadge + badgeWithRank.GoldBadge
						};
					}
					else
					{
						agentWithBadgeList[personAndBadgeType] = badgeWithRank;
					}
				}
			}

			var totalBadgeVm = agentWithBadgeList == null
				? new List<BadgeViewModel>()
				: agentWithBadgeList.Select(x => new BadgeViewModel
				{
					BadgeType = x.Key.BadgeType,
					BronzeBadge = x.Value.BronzeBadge,
					SilverBadge = x.Value.SilverBadge,
					GoldBadge = x.Value.GoldBadge
				});
			return totalBadgeVm;
		}
	}
}