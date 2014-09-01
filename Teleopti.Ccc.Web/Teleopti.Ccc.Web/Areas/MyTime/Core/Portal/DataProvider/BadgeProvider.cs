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

		public IEnumerable<IAgentBadge> GetBadges()
		{
			IEnumerable<IAgentBadge> result = null;
			var person = _loggedOnUser.CurrentUser();
			if (person != null)
			{
				result = _badgeRepository.Find(person);
			}
			return result;
		}
	}
}