using System;
using Teleopti.Ccc.Domain.Analytics;
using Teleopti.Ccc.Domain.Repositories;

namespace Teleopti.Ccc.TestCommon.FakeRepositories
{
	public class FakeAnalyticsBusinessUnitRepository : IAnalyticsBusinessUnitRepository
	{
		public bool ReturnNull { get; set; }

		public AnalyticBusinessUnit Get(Guid businessUnitCode)
		{
			return ReturnNull ? null : new AnalyticBusinessUnit {BusinessUnitId = 1, DatasourceId = 1};
		}
	}
}