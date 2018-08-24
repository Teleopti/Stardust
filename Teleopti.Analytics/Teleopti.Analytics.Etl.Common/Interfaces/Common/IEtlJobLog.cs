using System;
using Teleopti.Analytics.Etl.Common.Interfaces.Transformer;

namespace Teleopti.Analytics.Etl.Common.Interfaces.Common
{
    public interface IEtlJobLog
    {
        int ScheduleId { get; }
        string TenantName { get; }
        DateTime StartTime { get; }
        DateTime EndTime { get; }
        int ScopeIdentity { get; }

        bool Init(int scheduleId, string tenantName, DateTime startTime, DateTime endTime);
        void Persist(IJobResult jobResult);
        void PersistJobStep(IJobStepResult jobStepResult);
    }
}