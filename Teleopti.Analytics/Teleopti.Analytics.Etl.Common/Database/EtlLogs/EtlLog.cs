using System;
using Teleopti.Analytics.Etl.Interfaces.Common;
using Teleopti.Analytics.Etl.Interfaces.Transformer;


namespace Teleopti.Analytics.Etl.Common.Database.EtlLogs
{
    public class EtlLog : IEtlLog
    {
        private readonly ILogRepository _repository;

        public EtlLog()
        {

        }

        public EtlLog(ILogRepository rep)
        {
            _repository = rep;
        }


        public EtlLog(int scheduleId, DateTime startTime, DateTime endTime)
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

        public void AddJobStep(IJobStepResult jobStepResult)
        {
            _repository.AddJobStep(this, jobStepResult);
        }

        public int ScheduleId { get; private set; }

        public DateTime StartTime { get; private set; }

        public DateTime EndTime { get; private set; }



    }
}