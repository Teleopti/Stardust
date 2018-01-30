using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.ApplicationLayer.Rta.AgentAdherenceDay;

namespace Teleopti.Ccc.TestCommon.FakeRepositories
{
	public class FakeApprovedPeriodsStorage : IApprovedPeriodsReader
	{
		private readonly IList<ApprovedPeriodModel> _data = new List<ApprovedPeriodModel>();

		public void Has(ApprovedPeriodModel approvedPeriodModel)
		{
			_data.Add(approvedPeriodModel);
		}

		public IEnumerable<ApprovedPeriodModel> Read(Guid personId, DateTime startTime, DateTime endTime)
		{
			return _data
				.Where(x => x.PersonId == personId && x.StartTime >= startTime && x.EndTime <= endTime)
				.ToArray();
		}
	}
}