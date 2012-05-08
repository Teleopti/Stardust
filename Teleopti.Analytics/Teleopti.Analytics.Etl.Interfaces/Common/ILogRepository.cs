using System;
using System.Data;
using Teleopti.Analytics.Etl.Interfaces.Transformer;

namespace Teleopti.Analytics.Etl.Interfaces.Common
{
    public interface ILogRepository
    {        
        DataTable GetLog();
        int SaveLogPre();
        void SaveLogPost(IEtlJobLog etlJobLogItem, IJobResult jobResult); 
        void SaveLogStepPost(IEtlJobLog etlJobLogItem, IJobStepResult jobStepResult);
        void AddJobStep(IEtlJobLog etlJobLogItem, IJobStepResult jobStepResult);
    	DataTable GetEtlJobHistory(DateTime startDate, DateTime endDate, Guid businessUnitId);
    }
}