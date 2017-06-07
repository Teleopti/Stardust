using System;
using System.Collections.Generic;

namespace Teleopti.Ccc.Domain.ApplicationLayer.Rta.ReadModelUpdaters
{
	public interface ITeamsInAlarmReader
	{
		IEnumerable<TeamInAlarmModel> Read();
		IEnumerable<TeamInAlarmModel> Read(IEnumerable<Guid> skillIds);

		IEnumerable<TeamInAlarmModel> Read(Guid siteId);
		IEnumerable<TeamInAlarmModel> Read(Guid siteId, IEnumerable<Guid> skillIds);
	}
}