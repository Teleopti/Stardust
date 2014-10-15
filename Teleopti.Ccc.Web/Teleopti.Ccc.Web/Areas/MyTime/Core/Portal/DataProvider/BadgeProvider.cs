using System.Collections.Generic;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Web.Areas.MyTime.Core.Portal.DataProvider
{
	public class BadgeProvider : IBadgeProvider
	{
		private readonly ILoggedOnUser _loggedOnUser;
		private readonly IAgentBadgeRepository _badgeRepository;

		public BadgeProvider(ILoggedOnUser loggedOnUser, IAgentBadgeRepository badgeRepository)
		{
			_loggedOnUser = loggedOnUser;
			_badgeRepository = badgeRepository;
		}

		public IEnumerable<IAgentBadge> GetBadges(IAgentBadgeSettings settings)
		{
			var badges = new List<IAgentBadge>();
			var person = _loggedOnUser.CurrentUser();
			if (person == null) return badges;
			if (settings.AdherenceBadgeEnabled)
			{
				var badge = _badgeRepository.Find(person, BadgeType.Adherence);
				if (badge != null)
					badges.Add(badge);
			}
			if (settings.AHTBadgeEnabled)
			{
				var badge = _badgeRepository.Find(person, BadgeType.AverageHandlingTime);
				if (badge != null)
					badges.Add(badge);
			}
			if (settings.AnsweredCallsBadgeEnabled)
			{
				var badge = _badgeRepository.Find(person, BadgeType.AnsweredCalls);
				if (badge != null)
					badges.Add(badge);
			}
			return badges;
		}
	}
}