using System;
using Teleopti.Ccc.Domain.Analytics;

namespace Teleopti.Ccc.Domain.Repositories
{
	public interface IAnalyticsBusinessUnitRepository
	{
		AnalyticBusinessUnit Get(Guid businessUnitCode);
	}
}