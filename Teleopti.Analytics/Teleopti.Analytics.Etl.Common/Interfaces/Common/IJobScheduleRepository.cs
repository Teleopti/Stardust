using System.Data;

namespace Teleopti.Analytics.Etl.Common.Interfaces.Common
{
    public interface IJobScheduleRepository
    {
        DataTable GetSchedules();
        int SaveSchedule(IEtlJobSchedule etlJobScheduleItem);
        void DeleteSchedule(int scheduleId);
        DataTable GetSchedulePeriods(int scheduleId);
        void SaveSchedulePeriods(IEtlJobSchedule etlJobScheduleItem);
    }
}