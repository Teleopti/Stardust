using System.Collections.Generic;
using System.Linq;
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
		private readonly IToggleManager _toggleManager;
		private ITeamGamificationSetting _teamGamificationSetting;
		private readonly ITeamGamificationSettingRepository _teamGamificationSettingRepo;
		private readonly bool teamBasedGamificationSettingEnabled;

		public BadgeProvider(ILoggedOnUser loggedOnUser, IAgentBadgeRepository badgeRepository,
			IAgentBadgeWithRankRepository badgeWithRankRepository,
			ITeamGamificationSettingRepository teamGamificationSettingRepo,
			IToggleManager toggleManager)
		{
			_loggedOnUser = loggedOnUser;
			_badgeRepository = badgeRepository;
			_badgeWithRankRepository = badgeWithRankRepository;
			_toggleManager = toggleManager;
			_teamGamificationSettingRepo = teamGamificationSettingRepo;

			teamBasedGamificationSettingEnabled =
				_toggleManager.IsEnabled(Toggles.Portal_DifferentiateBadgeSettingForAgents_31318);
		}

		public IEnumerable<BadgeViewModel> GetBadges()
		{
			var badgeVmList = new List<BadgeViewModel>();
			var currentUser = _loggedOnUser.CurrentUser();
			if (currentUser == null) return badgeVmList;
			_teamGamificationSetting =
				_teamGamificationSettingRepo.FindTeamGamificationSettingsByTeam(currentUser.MyTeam(DateOnly.Today));
		
			var allBadges = getBadgesWithoutRank(currentUser);
			var allBadgesWithRank = getBadgesWithRank(currentUser);
				

			return mergeBadges(allBadges, allBadgesWithRank);
		}

		private IEnumerable<BadgeViewModel> getBadgesWithRank(IPerson person)
		{
			if (person == null)
			{
				return new List<BadgeViewModel>();
			}

			var badges = !teamBasedGamificationSettingEnabled
				? new List<IAgentBadgeWithRank>()
				: new List<IAgentBadgeWithRank>
				{
					_badgeWithRankRepository.Find(person, BadgeType.Adherence) ??
					new AgentBadgeWithRank
					{
						BadgeType = BadgeType.Adherence,
						Person = person.Id.Value,
						BronzeBadgeAmount = 0,
						SilverBadgeAmount = 0,
						GoldBadgeAmount = 0
					},
					_badgeWithRankRepository.Find(person, BadgeType.AverageHandlingTime) ??
					new AgentBadgeWithRank
					{
						BadgeType = BadgeType.Adherence,
						Person = person.Id.Value,
						BronzeBadgeAmount = 0,
						SilverBadgeAmount = 0,
						GoldBadgeAmount = 0
					},
					_badgeWithRankRepository.Find(person, BadgeType.AnsweredCalls) ??
					new AgentBadgeWithRank
					{
						BadgeType = BadgeType.Adherence,
						Person = person.Id.Value,
						BronzeBadgeAmount = 0,
						SilverBadgeAmount = 0,
						GoldBadgeAmount = 0
					}
				};

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
			if (!teamBasedGamificationSettingEnabled || person == null || _teamGamificationSetting == null)
			{
				return new List<BadgeViewModel>();
			}

			var badges = new List<AgentBadge>
			{
				_badgeRepository.Find(person, BadgeType.Adherence) ??
				new AgentBadge {BadgeType = BadgeType.Adherence, Person = person.Id.Value, TotalAmount = 0},
				_badgeRepository.Find(person, BadgeType.AverageHandlingTime) ??
				new AgentBadge {BadgeType = BadgeType.AverageHandlingTime, Person = person.Id.Value, TotalAmount = 0},
				_badgeRepository.Find(person, BadgeType.AnsweredCalls) ??
				new AgentBadge {BadgeType = BadgeType.AnsweredCalls, Person = person.Id.Value, TotalAmount = 0}
			};

			var setting = _teamGamificationSetting.GamificationSetting;
			var silverToBronzeBadgeRate = setting.SilverToBronzeBadgeRate;
			var goldToSilverBadgeRate = setting.GoldToSilverBadgeRate;
			var badgeVmList = badges.Select(x => new BadgeViewModel
			{
				BadgeType = x.BadgeType,
				BronzeBadge = x.GetBronzeBadge(silverToBronzeBadgeRate, goldToSilverBadgeRate),
				SilverBadge = x.GetSilverBadge(silverToBronzeBadgeRate, goldToSilverBadgeRate),
				GoldBadge = x.GetGoldBadge(silverToBronzeBadgeRate, goldToSilverBadgeRate)
			});
			return badgeVmList;
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