using System;
using Teleopti.Ccc.Domain.ApplicationLayer.ImportExternalPerformance;

namespace Teleopti.Ccc.Domain.ApplicationLayer.Events
{
	public class ImportExternalPerformanceInfoEvent : StardustJobInfo
	{
		public Guid JobResultId { get; set; }
		public ImportExternalPerformanceInfo ExternalPerformanceInfo { get; set; }
	}
}
