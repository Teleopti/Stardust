using System;

namespace Teleopti.Analytics.Etl.Common.Interfaces.PerformanceManager
{
	public class PmUser
	{
		public Guid PersonId { get; set; }
		public int AccessLevel { get; set; }
	}
}
