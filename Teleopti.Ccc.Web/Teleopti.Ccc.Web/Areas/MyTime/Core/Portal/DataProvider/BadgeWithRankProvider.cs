using System;
using System.Collections.Generic;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Web.Areas.MyTime.Core.Portal.DataProvider
{
	public class BadgeWithRankProvider : IBadgeWithRankProvider
	{
		private readonly ILoggedOnUser _loggedOnUser;
		private readonly IAgentBadgeWithRankRepository _badgeWithRankRepository;

		public BadgeWithRankProvider(ILoggedOnUser loggedOnUser, IAgentBadgeWithRankRepository badgeWithRankRepository)
		{
			_loggedOnUser = loggedOnUser;
			_badgeWithRankRepository = badgeWithRankRepository;
		}

		public IEnumerable<IAgentBadgeWithRank> GetBadges(IAgentBadgeSettings settings)
		{
			var badges = new List<IAgentBadgeWithRank>();
			var person = _loggedOnUser.CurrentUser();
			if (person == null) return badges;

			if (settings.AdherenceBadgeEnabled)
			{
				getBadge(person, BadgeType.Adherence, badges);
			}

			if (settings.AHTBadgeEnabled)
			{
				getBadge(person, BadgeType.AverageHandlingTime, badges);
			}

			if (settings.AnsweredCallsBadgeEnabled)
			{
				getBadge(person, BadgeType.AnsweredCalls, badges);
			}

			return badges;
		}

		private void getBadge(IPerson person, BadgeType badgeType, ICollection<IAgentBadgeWithRank> badges)
		{
			var badge = _badgeWithRankRepository.Find(person, badgeType);
			if (badge != null)
				badges.Add(badge);
		}
	}
}