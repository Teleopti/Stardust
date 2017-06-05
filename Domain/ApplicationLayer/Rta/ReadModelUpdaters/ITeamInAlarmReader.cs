using System;
using System.Collections.Generic;

namespace Teleopti.Ccc.Domain.ApplicationLayer.Rta.ReadModelUpdaters
{
	public interface ITeamInAlarmReader
	{
		IEnumerable<TeamInAlarmModel> Read(Guid siteId);
		IEnumerable<TeamInAlarmModel> Read(Guid siteId, IEnumerable<Guid> skillIds);
	}
}