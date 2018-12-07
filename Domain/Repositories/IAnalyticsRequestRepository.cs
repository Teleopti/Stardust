using System;
using System.Collections.Generic;
using Teleopti.Ccc.Domain.Analytics;

namespace Teleopti.Ccc.Domain.Repositories
{
	public interface IAnalyticsRequestRepository
	{
		void AddOrUpdate(AnalyticsRequest analyticsRequest);
		void AddOrUpdate(AnalyticsRequestedDay analyticsRequestedDay);
		IList<AnalyticsRequestedDay> GetAnalyticsRequestedDays(Guid requestId);
		void Delete(IEnumerable<AnalyticsRequestedDay> analyticsRequestedDays);
		void Delete(Guid requestId);
		void UpdateUnlinkedPersonids(int[] personPeriodIds);
	}
}