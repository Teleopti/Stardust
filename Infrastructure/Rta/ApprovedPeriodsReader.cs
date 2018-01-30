using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.ApplicationLayer.Rta.AgentAdherenceDay;

namespace Teleopti.Ccc.Infrastructure.Rta
{
	public class ApprovedPeriodsReader : IApprovedPeriodsReader
	{
		public IEnumerable<ApprovedPeriodModel> Read(Guid personId, DateTime startTime, DateTime endTime)
		{
			return Enumerable.Empty<ApprovedPeriodModel>();
		}
	}
}