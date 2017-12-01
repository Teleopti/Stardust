using System;

namespace Teleopti.Ccc.Domain.ApplicationLayer.Events
{
	public class ImportExternalPerformanceInfoEvent : StardustJobInfo
	{
		public Guid JobResultId { get; set; }
	}
}
