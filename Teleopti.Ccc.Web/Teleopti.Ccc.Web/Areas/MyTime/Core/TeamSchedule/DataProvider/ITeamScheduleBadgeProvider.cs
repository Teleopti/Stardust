using System;
using System.Collections.Generic;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Web.Areas.MyTime.Core.TeamSchedule.DataProvider
{
	public interface ITeamScheduleBadgeProvider
	{
		IEnumerable<IAgentBadge> GetBadges(Guid? personId);
	}
}