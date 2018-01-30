using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.ApplicationLayer.Rta.AgentAdherenceDay;

namespace Teleopti.Ccc.TestCommon.FakeRepositories
{
	public class FakeApprovedPeriodsStorage : IApprovedPeriodsReader
	{
		private readonly IList<ApprovedPeriod> _data = new List<ApprovedPeriod>();

		public void Has(ApprovedPeriod approvedPeriod)
		{
			_data.Add(approvedPeriod);
		}

		public IEnumerable<ApprovedPeriod> Read(Guid personId, DateTime startTime, DateTime endTime)
		{
			return _data
				.Where(x => x.PersonId == personId && x.StartTime >= startTime && x.EndTime <= endTime)
				.ToArray();
		}
	}
}