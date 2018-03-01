using System;
using System.Collections.Generic;
using Teleopti.Ccc.Domain.RealTimeAdherence.Domain.ApprovePeriodAsInAdherence;

namespace Teleopti.Ccc.Domain.RealTimeAdherence.Domain.AgentAdherenceDay
{
	public interface IApprovedPeriodsReader
	{
		IEnumerable<ApprovedPeriod> Read(Guid personId, DateTime startTime, DateTime endTime);
	}
}