using System;
using System.Collections.Generic;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Web.Areas.MyTime.Core.TeamSchedule.DataProvider
{
	public class TeamScheduleBadgeProvider : ITeamScheduleBadgeProvider
	{
		private readonly IAgentBadgeRepository _repository;

		public TeamScheduleBadgeProvider(IAgentBadgeRepository repository)
		{
			_repository = repository;
		}

		public IEnumerable<IAgentBadge> GetBadges(IPerson person)
		{
			IEnumerable<IAgentBadge> result = null;
			if (person != null)
			{
				result = _repository.Find(person);
			}
			return result;
		}
	}
}