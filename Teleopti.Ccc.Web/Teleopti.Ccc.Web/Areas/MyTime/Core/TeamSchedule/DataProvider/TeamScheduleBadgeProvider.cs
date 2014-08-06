using System;
using System.Collections.Generic;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Web.Areas.MyTime.Core.TeamSchedule.DataProvider
{
	public class TeamScheduleBadgeProvider : ITeamScheduleBadgeProvider
	{
		private readonly IPersonRepository _repository;

		public TeamScheduleBadgeProvider(IPersonRepository repository)
		{
			_repository = repository;
		}

		public IEnumerable<IAgentBadge> GetBadges(Guid? personId)
		{
			IEnumerable<IAgentBadge> result = null;
			if (personId != null)
			{
				var userId = (Guid) personId;
				result = _repository.Load(userId).Badges;
			}
			return result;
		}
	}
}