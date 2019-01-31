using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Forecasting;

namespace Teleopti.Ccc.TestCommon.FakeData
{
	public class FakeSystemJobStartTimeRepository : ISystemJobStartTimeRepository
	{
		public List<FakeStartTimeModel> List = new List<FakeStartTimeModel>();

		public DateTime? GetLastCalculatedTime(Guid bu, string jobName)
		{
			return List.FirstOrDefault(x => x.BusinessUnit == bu && x.JobName.Equals(jobName))?.StartedAt;
		}
	}

	public class FakeStartTimeModel
	{
		public DateTime StartedAt { get; set; }
		public Guid BusinessUnit { get; set; }
		public string JobName { get; set; }
	}
}