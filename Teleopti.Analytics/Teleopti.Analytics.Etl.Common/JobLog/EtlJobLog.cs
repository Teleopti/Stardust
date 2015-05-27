using System;
using Teleopti.Analytics.Etl.Common.Interfaces.Common;
using Teleopti.Analytics.Etl.Common.Interfaces.Transformer;

namespace Teleopti.Analytics.Etl.Common.JobLog
{
    public class EtlJobLog : IEtlJobLog
    {
        private readonly IJobLogRepository _repository;

        public EtlJobLog(IJobLogRepository rep)
        {
            _repository = rep;
        }


        public EtlJobLog(int scheduleId, DateTime startTime, DateTime endTime)
        {
            ScheduleId = scheduleId;
            StartTime = startTime;
            EndTime = endTime;
        }

        public void Init(int scheduleId, DateTime startTime, DateTime endTime)
        {
            ScheduleId = scheduleId;
            StartTime = startTime;
            EndTime = endTime;
            ScopeIdentity = _repository.SaveLogPre();
        }

        public int ScopeIdentity { get; private set; }

        public void PersistJobStep(IJobStepResult jobStepResult)
        {
            _repository.SaveLogStepPost(this, jobStepResult);
        }

        public void Persist(IJobResult jobResult)
        {
            _repository.SaveLogPost(this, jobResult);
        }

        public int ScheduleId { get; private set; }

        public DateTime StartTime { get; private set; }

        public DateTime EndTime { get; private set; }



    }
}