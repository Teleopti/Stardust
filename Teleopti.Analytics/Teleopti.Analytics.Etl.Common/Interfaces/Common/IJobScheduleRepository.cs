using System;
using System.Collections.Generic;
using System.Data;

namespace Teleopti.Analytics.Etl.Common.Interfaces.Common
{
    public interface IJobScheduleRepository
    {
		IList<IEtlJobSchedule> GetSchedules(IEtlJobLogCollection etlJobLogCollection, DateTime serverStartTime);
        int SaveSchedule(IEtlJobSchedule etlJobScheduleItem);
        void DeleteSchedule(int scheduleId);
        IList<IEtlJobRelativePeriod> GetSchedulePeriods(int scheduleId);
        void SaveSchedulePeriods(IEtlJobSchedule etlJobScheduleItem);
		void SetDataMartConnectionString(string connectionString);
		void DisableScheduleJob(int scheduleId);
	}

	
}