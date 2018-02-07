using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.ApplicationLayer.Rta.AgentAdherenceDay;

namespace Teleopti.Ccc.TestCommon.FakeRepositories
{
	public class FakeApprovedPeriodsStorage : IApprovedPeriodsReader,IApprovedPeriodsPersister
	{
		private readonly IList<ApprovedPeriod> _data = new List<ApprovedPeriod>();

		public void Has(ApprovedPeriod approvedPeriod)
		{
			_data.Add(approvedPeriod);
		}

		public IEnumerable<ApprovedPeriod> Data => _data;
		
		public IEnumerable<ApprovedPeriod> Read(Guid personId, DateTime startTime, DateTime endTime)
		{
			return _data
				.Where(x => x.PersonId == personId && x.StartTime >= startTime && x.EndTime <= endTime)
				.ToArray();
		}

		public void Persist(ApprovedPeriod approvedPeriod)
		{
			_data.Add(approvedPeriod);
		}
	}
}