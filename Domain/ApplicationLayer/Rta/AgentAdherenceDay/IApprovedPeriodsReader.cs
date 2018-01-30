using System;
using System.Collections.Generic;

namespace Teleopti.Ccc.Domain.ApplicationLayer.Rta.AgentAdherenceDay
{
	public interface IApprovedPeriodsReader
	{
		IEnumerable<ApprovedPeriodModel> Read(Guid personId, DateTime startTime, DateTime endTime);
	}
}