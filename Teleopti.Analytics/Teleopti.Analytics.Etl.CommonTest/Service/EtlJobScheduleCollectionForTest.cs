using System.Collections.Generic;
using Teleopti.Analytics.Etl.Common.Interfaces.Common;

namespace Teleopti.Analytics.Etl.CommonTest.Service
{
	public class EtlJobScheduleCollectionForTest : List<IEtlJobSchedule>, IEtlJobScheduleCollection
	{
		public EtlJobScheduleCollectionForTest(List<IEtlJobSchedule> jobSchedules)
		{
			this.AddRange(jobSchedules);
		}
	}
}