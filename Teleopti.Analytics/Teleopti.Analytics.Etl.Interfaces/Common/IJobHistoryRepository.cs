using System;
using System.Data;

namespace Teleopti.Analytics.Etl.Interfaces.Common
{
	public interface IJobHistoryRepository
	{
		DataTable GetEtlJobHistory(DateTime startDate, DateTime endDate, Guid businessUnitId, bool showOnlyErrors);
		DataTable BusinessUnitsIncludingAllItem { get; }
	}
}