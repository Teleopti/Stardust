using System;
using System.Collections.Generic;
using System.Data;
using Teleopti.Analytics.Etl.Interfaces.Common;


namespace Teleopti.Analytics.Etl.Common.Database.EtlLogs
{
    public class EtlLogCollection : List<IEtlLog>, IEtlLogCollection
    {
        public EtlLogCollection(ILogRepository repository)
        {
            Populate(repository);
        }

        private void Populate(ILogRepository rep)
        {
            DataTable dataTable = rep.GetLog();


            foreach (DataRow row in dataTable.Rows)
            {
                IEtlLog etlLog = new EtlLog((int)row["schedule_id"], (DateTime)row["job_start_time"], (DateTime)row["job_end_time"]); 
                Add(etlLog);
            }
        }
    }
}