using System;
using System.Data;
using Teleopti.Analytics.Etl.Interfaces.Transformer;

namespace Teleopti.Analytics.Etl.Interfaces.Common
{
    public interface ILogRepository
    {        
        DataTable GetLog();
        int SaveLogPre();
        void SaveLogPost(IEtlLog etlLogItem, IJobResult jobResult); 
        void SaveLogStepPost(IEtlLog etlLogItem, IJobStepResult jobStepResult);
        void AddJobStep(IEtlLog etlLogItem, IJobStepResult jobStepResult);
    	DataTable GetEtlJobHistory(DateTime startDate, DateTime endDate, Guid businessUnitId);
    }
}