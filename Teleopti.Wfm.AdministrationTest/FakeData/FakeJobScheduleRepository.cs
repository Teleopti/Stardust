using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Analytics.Etl.Common.Interfaces.Common;

namespace Teleopti.Wfm.AdministrationTest.FakeData
{
	public class FakeJobScheduleRepository : IJobScheduleRepository
	{
		private readonly IList<IEtlJobSchedule> _etlJobSchedules = new List<IEtlJobSchedule>();
		private readonly IDictionary<int, List<IEtlJobRelativePeriod>> _etlJobSchedulePeriods = new Dictionary<int, List<IEtlJobRelativePeriod>>();

		public IList<IEtlJobSchedule> GetSchedules(IEtlJobLogCollection etlJobLogCollection, DateTime serverStartTime)
		{
			return _etlJobSchedules;
		}

		public int SaveSchedule(IEtlJobSchedule jobSchedule)
		{
			DeleteSchedule(jobSchedule.ScheduleId);
			if (jobSchedule.ScheduleId == -1)
			{
				jobSchedule.SetScheduleIdOnPersistedItem(_etlJobSchedules.Any() ? _etlJobSchedules.Max(x => x.ScheduleId) + 1 : 1);
			}
			_etlJobSchedules.Add(jobSchedule);
			return jobSchedule.ScheduleId;
		}

		public void DeleteSchedule(int scheduleId)
		{
			var scheduleToDelete = _etlJobSchedules.FirstOrDefault(x => x.ScheduleId == scheduleId);
			if (scheduleToDelete == null)
				return;

			_etlJobSchedulePeriods.Remove(scheduleId);
			_etlJobSchedules.Remove(scheduleToDelete);
		}

		public IList<IEtlJobRelativePeriod> GetSchedulePeriods(int scheduleId)
		{
			return _etlJobSchedulePeriods[scheduleId];
		}

		public void SaveSchedulePeriods(IEtlJobSchedule etlJobScheduleItem)
		{
			if(!_etlJobSchedulePeriods.ContainsKey(etlJobScheduleItem.ScheduleId))
				_etlJobSchedulePeriods.Add(etlJobScheduleItem.ScheduleId, etlJobScheduleItem.RelativePeriodCollection.ToList());
		}

		public void SetDataMartConnectionString(string connectionString)
		{
			
		}

		public void ToggleScheduleJobEnabledState(int scheduleId)
		{
			var scheduleToToggle = _etlJobSchedules.FirstOrDefault(x => x.ScheduleId == scheduleId);
			scheduleToToggle?.SetEnabled(!scheduleToToggle.Enabled);
		}
	}
}
