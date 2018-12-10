using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Analytics;
using Teleopti.Ccc.Domain.Repositories;

namespace Teleopti.Ccc.TestCommon.FakeRepositories
{
	public class FakeAnalyticsRequestRepository : IAnalyticsRequestRepository
	{
		public readonly List<AnalyticsRequest> AnalyticsRequests = new List<AnalyticsRequest>();
		public readonly List<AnalyticsRequestedDay> AnalyticsRequestedDays = new List<AnalyticsRequestedDay>();

		public void AddOrUpdate(AnalyticsRequest analyticsRequest)
		{
			AnalyticsRequests.RemoveAll(x => x.RequestCode == analyticsRequest.RequestCode);
			AnalyticsRequests.Add(analyticsRequest);
		}

		public void AddOrUpdate(AnalyticsRequestedDay analyticsRequestedDay)
		{
			AnalyticsRequestedDays.RemoveAll(x => x.RequestCode == analyticsRequestedDay.RequestCode && x.RequestDateId == analyticsRequestedDay.RequestDateId);
			AnalyticsRequestedDays.Add(analyticsRequestedDay);
		}

		public IList<AnalyticsRequestedDay> GetAnalyticsRequestedDays(Guid requestId)
		{
			return AnalyticsRequestedDays.Where(x => x.RequestCode == requestId).ToList();
		}

		public void Delete(IEnumerable<AnalyticsRequestedDay> analyticsRequestedDays)
		{
			foreach (var analyticsRequestedDay in analyticsRequestedDays)
			{
				AnalyticsRequestedDays.RemoveAll(x => x.RequestCode == analyticsRequestedDay.RequestCode && x.RequestDateId == analyticsRequestedDay.RequestDateId);
			}
		}

		public void Delete(Guid requestId)
		{
			AnalyticsRequestedDays.RemoveAll(x => x.RequestCode == requestId);
			AnalyticsRequests.RemoveAll(x => x.RequestCode == requestId);
		}

		public void UpdateUnlinkedPersonids(int[] personPeriodIds)
		{
			throw new NotImplementedException();
		}
	}
}