using System;
using System.Collections.Generic;
using Teleopti.Ccc.Domain.Repositories;
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

		public IEnumerable<IAgentBadge> GetBadges(IAgentBadgeThresholdSettings settings)
		{
			var badges = new List<IAgentBadge>();
			var person = _loggedOnUser.CurrentUser();
			if (person == null) return badges;
			if (settings.AdherenceBadgeTypeSelected)
			{
				var badge = _badgeRepository.Find(person, BadgeType.Adherence);
				badges.Add(badge);
			}
			if (settings.AHTBadgeTypeSelected)
			{
				var badge = _badgeRepository.Find(person, BadgeType.AverageHandlingTime);
				badges.Add(badge);
			}
			if (settings.AnsweredCallsBadgeTypeSelected)
			{
				var badge = _badgeRepository.Find(person, BadgeType.AnsweredCalls);
				badges.Add(badge);
			}
			return badges;
		}
	}
}