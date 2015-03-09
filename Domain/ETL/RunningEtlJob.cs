using System;

namespace Teleopti.Ccc.Domain.ETL
{
	public class RunningEtlJob
	{
		public string ComputerName { get; set; }

		public DateTime StartTime { get; set; }

		public string JobName { get; set; }

		public bool IsStartedByService { get; set; }

		public DateTime LockUntil { get; set; }
	}
}