using System;
using System.Collections.Generic;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Web.Areas.MyTime.Core.Portal.DataProvider
{
	public class BadgeProvider : IBadgeProvider
	{
		private readonly ILoggedOnUser _loggedOnUser;
		private readonly IPersonRepository _repository;

		public BadgeProvider(ILoggedOnUser loggedOnUser, IPersonRepository repository)
		{
			_loggedOnUser = loggedOnUser;
			_repository = repository;
		}

		public IEnumerable<IAgentBadge> GetBadges()
		{
			IEnumerable<IAgentBadge> result = null;
			var guid = _loggedOnUser.CurrentUser().Id;
			if (guid != null)
			{
				var userId = (Guid) guid;
				result = _repository.Load(userId).Badges;
			}
			return result;
		}
	}
}