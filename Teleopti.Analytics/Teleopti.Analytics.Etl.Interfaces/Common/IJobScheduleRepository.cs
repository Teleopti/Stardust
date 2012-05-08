using System.Collections.Generic;
using System.Data;
using Teleopti.Analytics.Etl.Interfaces.Common;


namespace Teleopti.Analytics.Etl.Interfaces.Common
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