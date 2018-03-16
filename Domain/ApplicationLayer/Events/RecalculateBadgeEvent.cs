using System;

namespace Teleopti.Ccc.Domain.ApplicationLayer.Events
{
	public class RecalculateBadgeEvent : StardustJobInfo
	{
		public Guid JobResultId { get; set; }
		public DateTime StartDate { get; set; }
		public DateTime EndDate { get; set; }
	}
}
