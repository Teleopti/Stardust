using System;
using Teleopti.Ccc.Domain.Analytics;
using Teleopti.Ccc.Domain.Repositories;

namespace Teleopti.Ccc.TestCommon.FakeRepositories
{
	public class FakeAnalyticsBusinessUnitRepository : IAnalyticsBusinessUnitRepository
	{
		public AnalyticBusinessUnit Get(Guid businessUnitCode)
		{
			return new AnalyticBusinessUnit {BusinessUnitId = 1, DatasourceId = 1};
		}
	}
}