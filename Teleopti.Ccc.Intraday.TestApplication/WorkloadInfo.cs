using System;
using System.Collections.Generic;

namespace Teleopti.Ccc.Intraday.TestApplication
{
	public class WorkloadInfo
	{
		public int WorkloadId { get; set; }
		public IList<QueueInfo> Queues { get; set; }
		public string SkillName { get; set; }
		public Guid WorkloadCode { get; set; }
	}
}