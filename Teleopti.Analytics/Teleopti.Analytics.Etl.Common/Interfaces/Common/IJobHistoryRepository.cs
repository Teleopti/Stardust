using System;
using System.Collections.Generic;
using Teleopti.Analytics.Etl.Common.Entity;
using Teleopti.Analytics.Etl.Common.JobHistory;

namespace Teleopti.Analytics.Etl.Common.Interfaces.Common
{
	public interface IJobHistoryRepository
	{
		IList<JobHistoryViewModel> GetEtlJobHistory(DateTime startDate, DateTime endDate, Guid businessUnitId, bool showOnlyErrors, string connectionString);
		IList<BusinessUnitItem> GetBusinessUnitsIncludingAll(string connectionString);
	}
}