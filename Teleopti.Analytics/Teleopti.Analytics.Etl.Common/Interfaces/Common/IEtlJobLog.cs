using System;
using Teleopti.Analytics.Etl.Common.Interfaces.Transformer;

namespace Teleopti.Analytics.Etl.Common.Interfaces.Common
{
    public interface IEtlJobLog
    {
        int ScheduleId { get; }
        DateTime StartTime { get; }
        DateTime EndTime { get; }
        int ScopeIdentity { get; }

        void Init(int scheduleId, DateTime startTime, DateTime endTime);
        void Persist(IJobResult jobResult);
        void PersistJobStep(IJobStepResult jobStepResult);
        
    }
}