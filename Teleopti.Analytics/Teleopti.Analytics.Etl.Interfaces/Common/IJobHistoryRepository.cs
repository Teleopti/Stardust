using System;
using System.Data;

namespace Teleopti.Analytics.Etl.Interfaces.Common
{
	public interface IJobHistoryRepository
	{        
		DataTable GetEtlJobHistory(DateTime startDate, DateTime endDate, Guid businessUnitId);
		DataTable BusinessUnitsIncludingAllItem { get; }
	}
}