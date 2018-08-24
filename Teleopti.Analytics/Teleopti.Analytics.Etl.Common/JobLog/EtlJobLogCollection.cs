using System;
using System.Collections.Generic;
using System.Data;
using Teleopti.Analytics.Etl.Common.Interfaces.Common;

namespace Teleopti.Analytics.Etl.Common.JobLog
{
	public class EtlJobLogCollection : List<IEtlJobLog>, IEtlJobLogCollection
	{
		public EtlJobLogCollection(IJobLogRepository repository)
		{
			populate(repository);
		}

		private void populate(IJobLogRepository rep)
		{
			var dataTable = rep.GetLog();

			foreach (DataRow row in dataTable.Rows)
			{
				var tenantName = row["tenant_name"] == DBNull.Value ? "" : (string) row["tenant_name"];
				var etlJobLog = new EtlJobLog((int) row["schedule_id"], tenantName, (DateTime) row["job_start_time"],
					(DateTime) row["job_end_time"]);
				Add(etlJobLog);
			}
		}
	}
}