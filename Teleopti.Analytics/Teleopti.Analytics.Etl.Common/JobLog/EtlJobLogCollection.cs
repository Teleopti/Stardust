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
            Populate(repository);
        }

        private void Populate(IJobLogRepository rep)
        {
            DataTable dataTable = rep.GetLog();


            foreach (DataRow row in dataTable.Rows)
            {
                IEtlJobLog etlJobLog = new EtlJobLog((int)row["schedule_id"], (DateTime)row["job_start_time"], (DateTime)row["job_end_time"]); 
                Add(etlJobLog);
            }
        }
    }
}