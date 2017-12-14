using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Teleopti.Analytics.Etl.Common.Interfaces.Common;

namespace Teleopti.Wfm.AdministrationTest.FakeData
{
	public class FakeJobScheduleRepository : IJobScheduleRepository
	{
		private IList<IEtlJobSchedule> _etlJobSchedules = new List<IEtlJobSchedule>();
		private IDictionary<int, List<IEtlJobRelativePeriod>> _etlJobSchedulePeriods = new Dictionary<int, List<IEtlJobRelativePeriod>>();
		public DataTable GetSchedules()
		{
			throw new NotImplementedException();
		}

		public int SaveSchedule(IEtlJobSchedule etlJobScheduleItem)
		{
			_etlJobSchedules.Add(etlJobScheduleItem);
			return _etlJobSchedules.Count;
		}

		public void DeleteSchedule(int scheduleId)
		{
			throw new NotImplementedException();
		}

		public DataTable GetSchedulePeriods(int scheduleId)
		{
			throw new NotImplementedException();
		}

		public void SaveSchedulePeriods(IEtlJobSchedule etlJobScheduleItem)
		{
			if(!_etlJobSchedulePeriods.ContainsKey(etlJobScheduleItem.ScheduleId))
				_etlJobSchedulePeriods.Add(etlJobScheduleItem.ScheduleId, etlJobScheduleItem.RelativePeriodCollection.ToList());
		}

		public void SetDataMartConnectionString(string connectionString)
		{
			
		}

		public void DisableScheduleJob(int scheduleId)
		{
			throw new NotImplementedException();
		}

		public IList<IEtlJobSchedule> GetEtlJobSchedules()
		{
			return _etlJobSchedules;
		}

		public IList<IEtlJobRelativePeriod> GetEtlJobSchedulePeriods(int scheduleId)
		{
			return _etlJobSchedulePeriods[scheduleId];
		}
	}
}
