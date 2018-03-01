using System;
using System.Collections.Generic;
using Teleopti.Ccc.Domain.Rta.ApprovePeriodAsInAdherence;

namespace Teleopti.Ccc.Domain.Rta.AgentAdherenceDay
{
	public interface IApprovedPeriodsReader
	{
		IEnumerable<ApprovedPeriod> Read(Guid personId, DateTime startTime, DateTime endTime);
	}
}