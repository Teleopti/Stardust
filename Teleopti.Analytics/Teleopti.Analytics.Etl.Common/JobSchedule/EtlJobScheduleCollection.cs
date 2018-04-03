using System;
using System.Collections.Generic;
using Teleopti.Analytics.Etl.Common.Interfaces.Common;

namespace Teleopti.Analytics.Etl.Common.JobSchedule
{
	public class EtlJobScheduleCollection : List<IEtlJobSchedule>, IEtlJobScheduleCollection
	{
		public EtlJobScheduleCollection(IJobScheduleRepository repository, IEtlJobLogCollection etlJobLogCollection, DateTime serverStartTime)
		{
			Populate(repository, etlJobLogCollection, serverStartTime);
		}

		public void Populate(IJobScheduleRepository rep, IEtlJobLogCollection etlJobLogCollection, DateTime serverStartTime)
		{
			var jobs = rep.GetSchedules(etlJobLogCollection, serverStartTime);

			foreach (var job in jobs)
			{
				Add(job);
			}
		}
	}
}