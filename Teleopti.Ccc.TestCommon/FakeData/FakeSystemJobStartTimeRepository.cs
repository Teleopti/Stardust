using System;
using System.Collections.Generic;
using System.Linq;
using Google.Apis.Util;
using Teleopti.Ccc.Domain.Forecasting;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

namespace Teleopti.Ccc.TestCommon.FakeData
{
	public class FakeSystemJobStartTimeRepository : ISystemJobStartTimeRepository
	{
		private readonly INow _now;
		public List<FakeStartTimeModel> EntryList = new List<FakeStartTimeModel>();

		public FakeSystemJobStartTimeRepository(INow now)
		{
			_now = now;
		}

		public DateTime? GetLastCalculatedTime(Guid bu, string jobName)
		{
			return EntryList.FirstOrDefault(x => x.BusinessUnit == bu && x.JobName.Equals(jobName))?.StartedAt;
		}

		public void UpdateLastCalculatedTime(Guid bu, string jobName)
		{
			var entry = EntryList.SingleOrDefault(x => x.BusinessUnit == bu && x.JobName.Equals(jobName));
			if (entry != null)
			{
				entry.StartedAt = _now.UtcDateTime();
			}
			else
			{
				EntryList.Add(new FakeStartTimeModel()
				{
					StartedAt = _now.UtcDateTime(),
					BusinessUnit = bu,
					JobName = JobNamesForJoStartTime.TriggerSkillForecastReadModel
				});
			}
			
		}

	}


	public class FakeStartTimeModel
	{
		public DateTime StartedAt { get; set; }
		public Guid BusinessUnit { get; set; }
		public string JobName { get; set; }
	}
}